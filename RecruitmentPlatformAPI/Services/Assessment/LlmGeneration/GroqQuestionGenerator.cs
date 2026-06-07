using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;

namespace RecruitmentPlatformAPI.Services.Assessment.LlmGeneration
{
    public class GroqQuestionGenerator : ILlmQuestionGenerator
    {
        private readonly HttpClient _httpClient;
        private readonly LlmSettings _settings;
        private readonly ILogger<GroqQuestionGenerator> _logger;

        // ─────────────────────────────────────────────────────────────
        //  SYSTEM PROMPT — Few-shot based (~400 tokens)
        //  Instead of 10+ rules the LLM ignores, we SHOW what we want.
        // ─────────────────────────────────────────────────────────────
        private const string SystemPrompt =
@"You write MCQ assessment questions for a hiring platform.

GOOD QUESTION EXAMPLES (imitate these):

Example 1 (code-based):
{""questionText"":""Given the following code:\n```javascript\napp.get('/users', async (req, res) => {\n  const data = await db.query('SELECT * FROM users');\n  res.json(data);\n});\n```\nIf `db.query` rejects, what happens?"",""options"":[""Express returns a 500 with the error message"",""The process crashes with an unhandled promise rejection"",""Express silently ignores the error"",""The request hangs until the client times out""],""correctAnswerIndex"":1,""explanation"":""Unhandled async rejections in Express routes crash the process because Express doesn't catch promise rejections automatically.""}

Example 2 (scenario):
{""questionText"":""A teammate's PR changes the user table schema but doesn't update the API serializer. After merging, GET /users returns 200 but with missing fields. What is the root cause?"",""options"":[""The database migration failed"",""The API serializer still maps the old column names"",""The GET route has a caching bug"",""The frontend is filtering out fields""],""correctAnswerIndex"":1,""explanation"":""When schema changes aren't reflected in the serializer, it maps stale column names, causing missing fields.""}

BAD QUESTIONS (NEVER write these):
- ""What happens when you use console.log()?"" → TOO TRIVIAL
- ""What is the primary function of package.json?"" → DEFINITION RECALL
- ""What happens when X if X doesn't exist?"" → ANSWER IS IN THE QUESTION

RULES:
1. Every question needs a CONCISE technical situation (Max 1-2 sentences). Be direct. Do NOT use wordy role-play setups like ""You are tasked with..."" or ""You are building a..."".
2. Options must be plausible near-misses from the SAME technology. Never use obviously wrong/absurd options.
3. The correct answer must NOT appear as a keyword in the question stem.
4. For SoftSkill category: use TECHNICAL team scenarios (broken builds, code review conflicts, deploy failures) — never generic HR advice.
5. questionText: 50-600 chars. Options: 5-150 chars. Explanation: 1 sentence.
6. Role Match: The scenario MUST match the candidate's Level. Do NOT ask Junior/Mid candidates how to mentor or lead others.
7. Code Context & Formatting: Use single backticks for short inline code (`code`). For multi-line code, use fenced code blocks (```language\ncode\n```). CRITICAL: Ensure all newlines inside code blocks are properly escaped as \\n in the JSON string!
8. Distinct Options: Options MUST be mutually exclusive. Do not include two options that mean the same thing.
9. Constraints: If a technical constraint is given (e.g., 'sorted array'), the correct answer MUST utilize that constraint for the most optimal solution.

OUTPUT: JSON object with key ""questions"" — array of:
{""questionText"":string,""category"":""Technical""|""SoftSkill"",""difficulty"":""Easy""|""Medium""|""Hard"",""skillName"":string,""topicTag"":string,""options"":[str,str,str,str],""correctAnswerIndex"":0|1|2|3,""explanation"":string}
Return ONLY valid JSON.";

        private const int MaxQuestionsPerBatch = 5;
        private const int InterBatchDelayMs = 10000;

        public GroqQuestionGenerator(
            HttpClient httpClient,
            IOptions<LlmSettings> settings,
            ILogger<GroqQuestionGenerator> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════════
        //  PUBLIC ENTRY POINT
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<LlmGeneratedQuestionDto>> GenerateQuestionsAsync(
            GenerationRequest request, CancellationToken ct)
        {
            var allResults = new List<LlmGeneratedQuestionDto>();
            var chunks = SplitDistribution(request.Distribution, MaxQuestionsPerBatch);
            var coveredTopics = new List<string>(request.AlreadyCoveredTopics);

            _logger.LogInformation("Generating {Total} questions in {Batches} batches.",
                request.TotalQuestions, chunks.Count);

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var batchRequest = new GenerationRequest
                {
                    JobTitle = request.JobTitle,
                    RoleFamily = request.RoleFamily,
                    SeniorityLevel = request.SeniorityLevel,
                    YearsOfExperience = request.YearsOfExperience,
                    ClaimedSkillNames = request.ClaimedSkillNames,
                    Distribution = chunk,
                    TotalQuestions = chunk.Sum(d => d.Count),
                    AlreadyCoveredTopics = coveredTopics
                };

                var batchResults = await GenerateSingleBatchAsync(batchRequest, ct);
                allResults.AddRange(batchResults);

                // Harvest topics for cross-batch dedup
                coveredTopics.AddRange(batchResults
                    .Where(q => !string.IsNullOrWhiteSpace(q.TopicTag))
                    .Select(q => q.TopicTag!));

                // Proactive delay between batches to avoid 429s
                if (i < chunks.Count - 1)
                {
                    _logger.LogInformation("Waiting {Delay}ms between batches.", InterBatchDelayMs);
                    await Task.Delay(InterBatchDelayMs, ct);
                }
            }

            return allResults;
        }

        // ═══════════════════════════════════════════════════════════════
        //  SINGLE BATCH — one API call with retries
        // ═══════════════════════════════════════════════════════════════
        private async Task<List<LlmGeneratedQuestionDto>> GenerateSingleBatchAsync(
            GenerationRequest request, CancellationToken ct)
        {
            var allValid = new List<LlmGeneratedQuestionDto>();
            int target = request.TotalQuestions;
            string url = $"{_settings.BaseUrl.TrimEnd('/')}/chat/completions";

            for (int attempt = 1; attempt <= _settings.MaxRetries && allValid.Count < target; attempt++)
            {
                try
                {
                    int remaining = target - allValid.Count;
                    _logger.LogInformation("Batch attempt {Attempt}: requesting {Count} questions (have {Have}/{Target}).",
                        attempt, remaining, allValid.Count, target);

                    var scaledDist = attempt == 1
                        ? request.Distribution
                        : ScaleDistribution(request.Distribution, remaining);

                    // Build covered topics from outer + already harvested
                    var covered = request.AlreadyCoveredTopics.ToList();
                    covered.AddRange(allValid
                        .Where(q => !string.IsNullOrWhiteSpace(q.TopicTag))
                        .Select(q => q.TopicTag!));

                    string userPrompt = BuildUserPrompt(request, scaledDist, remaining, covered);

                    // Token budget: ~200 tokens per question
                    int maxTokens = Math.Min(4000, Math.Max(600, remaining * 200));

                    var body = new
                    {
                        model = _settings.Model,
                        messages = new[]
                        {
                            new { role = "system", content = SystemPrompt },
                            new { role = "user", content = userPrompt }
                        },
                        temperature = 0.4,
                        max_tokens = maxTokens,
                        response_format = new { type = "json_object" }
                    };

                    using var httpReq = new HttpRequestMessage(HttpMethod.Post, url);
                    httpReq.Content = JsonContent.Create(body);
                    httpReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

                    var response = await _httpClient.SendAsync(httpReq, ct);

                    // ── Handle errors ──
                    if (!response.IsSuccessStatusCode)
                    {
                        string err = await response.Content.ReadAsStringAsync(ct);
                        _logger.LogWarning("API error (attempt {A}/{Max}): {Status} — {Err}",
                            attempt, _settings.MaxRetries, response.StatusCode,
                            err.Length > 200 ? err[..200] : err);

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            // On last attempt with some results, accept what we have
                            if (attempt >= _settings.MaxRetries && allValid.Count > 0)
                            {
                                _logger.LogWarning("Rate limited on final attempt. Returning {Count}/{Target} questions.",
                                    allValid.Count, target);
                                break;
                            }

                            int delay = ParseRetryAfter(response) ?? 15000;
                            _logger.LogWarning("Rate limited. Waiting {Delay}ms.", delay);
                            await Task.Delay(delay, ct);
                            continue;
                        }

                        if (attempt >= _settings.MaxRetries)
                            throw new LlmGenerationException($"API returned {response.StatusCode}", err);

                        await Task.Delay(_settings.RetryDelayMs * attempt, ct);
                        continue;
                    }

                    // ── Parse response ──
                    var groqResp = await response.Content.ReadFromJsonAsync<GroqResponseEnvelope>(cancellationToken: ct);
                    string? raw = groqResp?.Choices?.FirstOrDefault()?.Message?.Content;

                    if (string.IsNullOrEmpty(raw))
                    {
                        _logger.LogWarning("Empty response (attempt {A}).", attempt);
                        continue;
                    }

                    // ── Validate & deduplicate ──
                    var validated = ParseAndValidate(raw);
                    if (!validated.Any())
                    {
                        _logger.LogWarning("Zero valid questions from response (attempt {A}).", attempt);
                        continue;
                    }

                    var deduped = Deduplicate(validated, allValid);
                    allValid.AddRange(deduped);

                    _logger.LogInformation("Harvested {New} valid questions ({Rejected} rejected). Total: {Total}/{Target}.",
                        deduped.Count, validated.Count - deduped.Count, allValid.Count, target);
                }
                catch (Exception ex) when (ex is not LlmGenerationException)
                {
                    _logger.LogError(ex, "Exception in batch attempt {A}.", attempt);
                    if (attempt >= _settings.MaxRetries) throw;
                    await Task.Delay(_settings.RetryDelayMs * attempt, ct);
                }
            }

            // Accept ≥70% of target (rate-limit resilience)
            int threshold = (int)Math.Ceiling(target * 0.70);
            if (allValid.Count >= threshold)
            {
                if (allValid.Count < target)
                    _logger.LogWarning("Returning {Count}/{Target} questions (≥70% threshold).", allValid.Count, target);

                var result = allValid.Take(target).ToList();
                ShuffleOptions(result);
                return result;
            }

            throw new LlmGenerationException(
                $"Failed to generate enough questions. Got {allValid.Count}/{target} (need {threshold}).");
        }

        // ═══════════════════════════════════════════════════════════════
        //  USER PROMPT — compact (~100 tokens)
        // ═══════════════════════════════════════════════════════════════
        private static string BuildUserPrompt(
            GenerationRequest req, List<QuestionDistributionItem> dist, int count, List<string> covered)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Skills: {string.Join(", ", req.ClaimedSkillNames)}");
            sb.AppendLine($"Level: {req.SeniorityLevel} ({req.YearsOfExperience} yrs)");

            // Compact distribution: "Node.js:Easy×2,Medium×1; Express.js:Medium×2"
            var grouped = dist.GroupBy(d => d.SkillName)
                .Select(g => $"{g.Key}:{string.Join(",", g.Select(d => $"{d.Difficulty}×{d.Count}"))}")
                .ToList();
            sb.AppendLine($"Distribution: {string.Join("; ", grouped)}");

            if (covered.Count > 0)
                sb.AppendLine($"AVOID these topics (already covered): {string.Join(", ", covered.Distinct().Take(20))}");

            sb.AppendLine($"Generate exactly {count} questions as JSON.");

            return sb.ToString();
        }

        // ═══════════════════════════════════════════════════════════════
        //  PARSE & VALIDATE
        // ═══════════════════════════════════════════════════════════════
        private List<LlmGeneratedQuestionDto> ParseAndValidate(string rawText)
        {
            try
            {
                using var doc = JsonDocument.Parse(rawText);
                JsonElement arr;

                if (doc.RootElement.TryGetProperty("questions", out var qProp))
                    arr = qProp;
                else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    arr = doc.RootElement;
                else
                {
                    _logger.LogWarning("Response missing 'questions' key.");
                    return new List<LlmGeneratedQuestionDto>();
                }

                var questions = JsonSerializer.Deserialize<List<LlmGeneratedQuestionDto>>(
                    arr.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (questions == null) return new List<LlmGeneratedQuestionDto>();

                var valid = new List<LlmGeneratedQuestionDto>();
                foreach (var q in questions)
                {
                    var reason = GetRejectionReason(q);
                    if (reason != null)
                    {
                        _logger.LogWarning("Rejected [{Reason}]: {Text}",
                            reason, q.QuestionText?[..Math.Min(80, q.QuestionText?.Length ?? 0)] ?? "(null)");
                        continue;
                    }
                    valid.Add(q);
                }
                return valid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse LLM response ({Len} chars).", rawText?.Length ?? 0);
                return new List<LlmGeneratedQuestionDto>();
            }
        }

        /// <summary>
        /// Returns null if the question is valid, or a reason string if it should be rejected.
        /// </summary>
        private static string? GetRejectionReason(LlmGeneratedQuestionDto q)
        {
            // ── Structural checks ──
            if (string.IsNullOrWhiteSpace(q.QuestionText))
                return "empty question";
            if (q.QuestionText.Length < 50 || q.QuestionText.Length > 600)
                return $"length {q.QuestionText.Length} (need 50-600)";
            if (!q.QuestionText.TrimEnd().EndsWith('?'))
                return "missing question mark";
            if (q.Category != "Technical" && q.Category != "SoftSkill")
                return $"bad category '{q.Category}'";
            if (q.Difficulty != "Easy" && q.Difficulty != "Medium" && q.Difficulty != "Hard")
                return $"bad difficulty '{q.Difficulty}'";
            if (q.Options == null || q.Options.Count != 4 || q.Options.Any(string.IsNullOrWhiteSpace))
                return "bad options";
            if (q.Options.Any(o => o.Length < 3))
                return "option too short";
            if (q.CorrectAnswerIndex < 0 || q.CorrectAnswerIndex > 3)
                return $"bad correctIndex {q.CorrectAnswerIndex}";
            if (string.IsNullOrWhiteSpace(q.Explanation) || q.Explanation.Length < 10 || q.Explanation.Length > 300)
                return $"explanation length {q.Explanation?.Length ?? 0}";
            if (q.Options.Any(o => o.ToLowerInvariant().Contains("all of the above") || o.ToLowerInvariant().Contains("none of the above")))
                return "uses 'all/none of the above' option";

            // ── Content quality checks ──

            var lower = q.QuestionText.ToLowerInvariant();

            // 1. Definitional / trivia patterns
            if (IsDefinitionalQuestion(lower))
                return "definitional/trivia";

            // 2. Answer embedded in the question stem
            if (IsAnswerInQuestion(q))
                return "answer in question stem";

            // 3. Trivial "What happens when you use X?" with obvious answer
            if (IsTrivialWhatHappens(q))
                return "trivial 'what happens'";

            // 4. All options are very short single-concept answers (pure recall)
            if (q.Options.All(o => o.Length < 20) && q.Category == "Technical")
            {
                // Allow if it's a code-based question
                if (!lower.Contains('`'))
                    return "all options <20 chars without code snippet";
            }

            // 5. Weak distractors
            if (HasWeakDistractors(q))
                return "weak/wrong-category distractors";

            // ── Advisory fixes (don't reject) ──
            if (string.IsNullOrWhiteSpace(q.TopicTag) || q.TopicTag.Length > 60)
                q.TopicTag = $"{q.SkillName?.ToLower().Replace(" ", "-")}-general";

            return null;
        }

        // ═══════════════════════════════════════════════════════════════
        //  QUALITY FILTERS
        // ═══════════════════════════════════════════════════════════════

        private static bool IsDefinitionalQuestion(string lower)
        {
            // If the question contains a code snippet or backticks, it is almost certainly a specific technical scenario
            if (lower.Contains("`"))
                return false;

            // Mid-sentence definition patterns
            var defPatterns = new[]
            {
                "what is the role of", "what is the function of",
                "what is the primary", "what is the main", "what is the key",
                "what are the differences", "what is the definition of", "what does the"
            };
            if (defPatterns.Any(p => lower.Contains(p)))
                return true;

            // Start-of-string definition
            if (lower.StartsWith("what is ") || lower.StartsWith("what are ")
                || lower.StartsWith("define ") || lower.StartsWith("explain what")
                || lower.StartsWith("which of the following defines"))
                return true;

            // Banned phrases — subjective or multi-answer patterns
            var banned = new[] {
                "primary purpose", "primary function", "primary benefit", "primary role",
                "best practice", "most suitable", "most appropriate",
                // "best" wording creates subjective questions with multiple valid answers
                "what is the best", "what would be the best"
            };
            if (banned.Any(b => lower.Contains(b)))
                return true;

            return false;
        }

        /// <summary>
        /// Rejects questions where the correct answer text appears verbatim in the question.
        /// Example: "Why does readFile fail if the file does not exist?" → answer: "The file does not exist"
        /// </summary>
        private static bool IsAnswerInQuestion(LlmGeneratedQuestionDto q)
        {
            if (q.CorrectAnswerIndex < 0 || q.CorrectAnswerIndex >= q.Options.Count)
                return false;

            string answer = q.Options[q.CorrectAnswerIndex].ToLowerInvariant().Trim();
            string stem = q.QuestionText.ToLowerInvariant();

            // Skip if answer is very short (common words like "true", "null")
            if (answer.Length < 15) return false;

            // Check if the core of the answer appears in the stem
            // Strip leading articles/pronouns
            var core = answer
                .Replace("it ", "").Replace("the ", "").Replace("a ", "")
                .Trim();

            return core.Length >= 12 && stem.Contains(core);
        }

        /// <summary>
        /// Catches trivially easy "What happens when you use X?" questions
        /// where any developer would know the answer instantly.
        /// </summary>
        private static bool IsTrivialWhatHappens(LlmGeneratedQuestionDto q)
        {
            var lower = q.QuestionText.ToLowerInvariant();

            // Pattern: "What happens when you [simple verb] [basic tool]?"
            // without any error/constraint/consequence context
            if (!lower.StartsWith("what happens when"))
                return false;

            // If the question contains error/bug/constraint context, it's not trivial
            var complexityMarkers = new[]
            {
                "error", "fail", "crash", "bug", "broken", "unexpected", "instead of",
                "but ", "however", "conflict", "reject", "invalid", "timeout", "already",
                "multiple times", "simultaneously", "concurrently", "without", "missing"
            };

            if (complexityMarkers.Any(m => lower.Contains(m)))
                return false;

            // It's a trivial "what happens when you use X" — reject
            return true;
        }

        private static bool HasWeakDistractors(LlmGeneratedQuestionDto q)
        {
            if (q.Options == null || q.Options.Count != 4
                || q.CorrectAnswerIndex < 0 || q.CorrectAnswerIndex > 3)
                return false;

            string correct = q.Options[q.CorrectAnswerIndex].ToLowerInvariant();
            var wrongs = q.Options
                .Where((_, i) => i != q.CorrectAnswerIndex)
                .Select(o => o.ToLowerInvariant())
                .ToList();

            // Wrong-category databases
            var dbs = new[] { "mysql", "mongodb", "redis", "sqlite", "oracle",
                "cassandra", "dynamodb", "mariadb", "postgresql", "postgres", "sql server" };
            if (dbs.Any(d => correct.Contains(d)) && wrongs.Count(o => dbs.Any(d => o.Contains(d))) >= 2)
                return true;

            // All 4 options are plain HTTP verbs (pure recall)
            var verbs = new[] { "get", "post", "put", "delete", "patch" };
            if (q.Options.Count(o => verbs.Any(v => o.Trim().Equals(v, StringComparison.OrdinalIgnoreCase)
                || o.Trim().Equals($"app.{v}()", StringComparison.OrdinalIgnoreCase))) >= 4)
                return true;

            // Wrong-category frameworks
            var fw = new[] { "django", "flask", "spring", "rails", "laravel", "fastapi", "express", "koa" };
            if (fw.Any(f => correct.Contains(f)) && wrongs.Count(o => fw.Any(f => o.Contains(f))) >= 2)
                return true;

            return false;
        }

        // ═══════════════════════════════════════════════════════════════
        //  DEDUPLICATION
        // ═══════════════════════════════════════════════════════════════

        private List<LlmGeneratedQuestionDto> Deduplicate(
            List<LlmGeneratedQuestionDto> incoming, List<LlmGeneratedQuestionDto> existing)
        {
            var usedPrefixes = existing
                .Select(q => GetOpeningPrefix(q.QuestionText))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var usedConcepts = existing
                .Select(q => GetConceptFingerprint(q))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var usedNormalized = existing
                .Select(q => NormalizeForDedup(q.QuestionText))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Stem keyword sets — catches near-duplicates like Q11/Q12/Q16 that all ask
            // about "inconsistent data" with different phrasing. If 5+ significant words
            // overlap between two question stems, they test the same scenario.
            var existingStemKeywords = existing
                .Select(q => GetStemKeywords(q.QuestionText))
                .ToList();

            var result = new List<LlmGeneratedQuestionDto>();
            foreach (var q in incoming)
            {
                var prefix = GetOpeningPrefix(q.QuestionText);
                var concept = GetConceptFingerprint(q);
                var normalized = NormalizeForDedup(q.QuestionText);
                var stemKeys = GetStemKeywords(q.QuestionText);

                if (!usedPrefixes.Add(prefix))
                {
                    _logger.LogWarning("Dedup: same prefix '{P}'.", prefix);
                    continue;
                }
                if (!usedConcepts.Add(concept))
                {
                    _logger.LogWarning("Dedup: same concept '{C}'.", concept);
                    continue;
                }
                if (!usedNormalized.Add(normalized))
                {
                    _logger.LogWarning("Dedup: same normalized text.");
                    continue;
                }
                // Check keyword overlap against all accepted questions (existing + this batch)
                bool tooSimilar = existingStemKeywords.Any(existingKeys =>
                    stemKeys.Count > 0 && existingKeys.Count > 0 &&
                    stemKeys.Intersect(existingKeys, StringComparer.OrdinalIgnoreCase).Count() >= 5);
                if (tooSimilar)
                {
                    _logger.LogWarning("Dedup: stem keyword overlap (near-duplicate scenario): {Text}",
                        q.QuestionText[..Math.Min(60, q.QuestionText.Length)]);
                    continue;
                }

                existingStemKeywords.Add(stemKeys);
                result.Add(q);
            }
            return result;
        }

        /// <summary>
        /// Extracts significant nouns/verbs from a question stem for overlap-based deduplication.
        /// Strips stopwords so "A REST API endpoint returns inconsistent data when queried concurrently"
        /// and "A REST API endpoint returns inconsistent data" produce nearly the same keyword set.
        /// </summary>
        private static HashSet<string> GetStemKeywords(string text)
        {
            var stopwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a", "an", "the", "is", "are", "was", "were", "be", "been", "being",
                "have", "has", "had", "do", "does", "did", "will", "would", "could",
                "should", "may", "might", "shall", "can", "to", "of", "in", "on", "at",
                "by", "for", "with", "about", "against", "between", "into", "through",
                "and", "but", "or", "nor", "not", "so", "yet", "both", "either",
                "if", "when", "where", "how", "what", "which", "who", "that", "this",
                "it", "its", "you", "your", "they", "their", "them", "he", "she",
                "due", "than", "then", "there", "from", "after", "before", "while"
            };

            return text.ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', '?', '!', '`', '"', '\'', '(', ')', '/', '-', '_' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3 && !stopwords.Contains(w))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static string GetOpeningPrefix(string text)
        {
            var words = text.TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', words.Take(5)).ToLowerInvariant();
        }

        private static string GetConceptFingerprint(LlmGeneratedQuestionDto q)
        {
            var skill = (q.SkillName ?? "x").ToLowerInvariant().Trim();
            var answer = (q.CorrectAnswerIndex >= 0 && q.CorrectAnswerIndex < q.Options.Count)
                ? q.Options[q.CorrectAnswerIndex].ToLowerInvariant()
                    .Replace("()", "").Replace(";", "").Replace("\"", "").Replace("'", "").Trim()
                : "";
            var tokens = answer.Split(new[] { ' ', '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            var key = tokens.Length > 0 ? tokens.Last() : "x";
            return $"{skill}:{key}";
        }

        /// <summary>
        /// Strips punctuation, backticks, and normalizes whitespace for near-duplicate detection.
        /// </summary>
        private static string NormalizeForDedup(string text)
        {
            var sb = new StringBuilder();
            foreach (var c in text.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c) || c == ' ')
                    sb.Append(c);
            }
            // Collapse multiple spaces
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString().Trim(), @"\s+", " ");
        }

        // ═══════════════════════════════════════════════════════════════
        //  DISTRIBUTION SPLITTING & SCALING
        // ═══════════════════════════════════════════════════════════════

        private List<List<QuestionDistributionItem>> SplitDistribution(
            List<QuestionDistributionItem> distribution, int maxPerBatch)
        {
            var chunks = new List<List<QuestionDistributionItem>>();
            var current = new List<QuestionDistributionItem>();
            int currentCount = 0;

            foreach (var item in distribution)
            {
                if (currentCount + item.Count > maxPerBatch && current.Any())
                {
                    chunks.Add(current);
                    current = new List<QuestionDistributionItem>();
                    currentCount = 0;
                }
                current.Add(item);
                currentCount += item.Count;
            }
            if (current.Any())
                chunks.Add(current);

            // Merge tiny trailing chunks (<3) into previous to save an API call
            if (chunks.Count >= 2 && chunks.Last().Sum(d => d.Count) < 3)
            {
                var last = chunks.Last();
                chunks.RemoveAt(chunks.Count - 1);
                chunks.Last().AddRange(last);
            }

            return chunks;
        }

        private static List<QuestionDistributionItem> ScaleDistribution(
            List<QuestionDistributionItem> original, int target)
        {
            int total = original.Sum(d => d.Count);
            if (target >= total) return original;

            if (target <= original.Count)
            {
                return original.OrderByDescending(d => d.Count).Take(target)
                    .Select(d => new QuestionDistributionItem
                    {
                        SkillName = d.SkillName, Category = d.Category,
                        Difficulty = d.Difficulty, Count = 1
                    }).ToList();
            }

            double ratio = (double)target / total;
            var scaled = original.Select(d => new QuestionDistributionItem
            {
                SkillName = d.SkillName, Category = d.Category,
                Difficulty = d.Difficulty, Count = Math.Max(1, (int)Math.Round(d.Count * ratio))
            }).ToList();

            // Drift correction
            int drift = scaled.Sum(s => s.Count) - target;
            while (drift > 0)
            {
                var r = scaled.Where(s => s.Count > 1).OrderByDescending(s => s.Count).FirstOrDefault();
                if (r == null) break;
                r.Count--;
                drift--;
            }
            while (drift < 0)
            {
                scaled.OrderByDescending(s => s.Count).First().Count++;
                drift++;
            }

            return scaled.Where(s => s.Count > 0).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════════════

        private static int? ParseRetryAfter(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Retry-After", out var vals)
                && int.TryParse(vals.FirstOrDefault(), out var secs))
                return (secs + 1) * 1000;
            return null;
        }

        private void ShuffleOptions(List<LlmGeneratedQuestionDto> questions)
        {
            foreach (var q in questions)
            {
                if (q.Options == null || q.Options.Count < 2) continue;
                string correctText = q.Options[q.CorrectAnswerIndex];

                for (int i = q.Options.Count - 1; i > 0; i--)
                {
                    int j = Random.Shared.Next(i + 1);
                    (q.Options[i], q.Options[j]) = (q.Options[j], q.Options[i]);
                }

                q.CorrectAnswerIndex = q.Options.IndexOf(correctText);
            }
        }

        // ── Groq API response DTOs ──
        private class GroqResponseEnvelope
        {
            [JsonPropertyName("choices")] public List<GroqChoice>? Choices { get; set; }
        }
        private class GroqChoice
        {
            [JsonPropertyName("message")] public GroqMessage? Message { get; set; }
        }
        private class GroqMessage
        {
            [JsonPropertyName("content")] public string? Content { get; set; }
        }
    }
}
