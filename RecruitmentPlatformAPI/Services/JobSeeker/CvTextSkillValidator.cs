using System.Text.RegularExpressions;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    /// <summary>
    /// Validates LLM-extracted skill names against the raw CV text to catch
    /// hallucinated skills (e.g. Gemini returning "Java" when the CV only says "JavaScript").
    ///
    /// Two-tier validation:
    ///   1. Confusable-pair check — for known pairs where one skill is a substring of another
    ///      (java/javascript, git/github, etc.), ensures the short skill appears standalone.
    ///   2. Generic presence check — the skill name must appear somewhere in the normalized CV text.
    /// </summary>
    public class CvTextSkillValidator
    {
        private readonly ILogger<CvTextSkillValidator> _logger;

        /// <summary>
        /// Dictionary of known confusable skill pairs.
        /// Key   = the short / hallucination-prone skill name (lowercase).
        /// Value = list of longer skill names that CONTAIN the key as a substring.
        ///
        /// When the LLM extracts the short form, we check whether every occurrence
        /// of it in the CV text is actually part of one of the longer forms.
        /// If so, the short form is a hallucination and we drop it.
        /// </summary>
        private static readonly Dictionary<string, List<string>> ConfusablePairs =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // "java" is a substring of "javascript"
                ["java"] = new() { "javascript", "javafx", "java script" },

                // "figjam" hallucinated when CV says "figma" — reverse direction:
                // if LLM says "figjam" but CV only contains "figma", drop it.
                // We handle this by checking presence; "figjam" won't appear in CV text
                // that only says "figma", so the generic check catches it.
                // But we also add the forward direction in case CV says "FigJam board"
                // and LLM extracts "Figma" instead:
                ["figma"] = new() { "figjam" },

                // "git" is a substring of "github", "gitlab", "gitflow", "gitbash"
                ["git"] = new() { "github", "gitlab", "gitflow", "gitbash", "gitea", "gitpod", "gitkraken" },

                // "c" as a language vs "c#", "c++", "css", "csv"
                ["c"] = new() { "c#", "c++", "c sharp", "css", "csv", "csharp" },

                // "r" as a language vs "react", "ruby", "rust", "redis"
                ["r"] = new() { "react", "ruby", "rust", "redis", "rabbitmq", "rails" },

                // "go" as a language vs "google", "golang" (golang IS go, so we accept it)
                // Note: "golang" should resolve to "go" via alias, but if CV says "google"
                // and LLM extracts "go", we want to reject it.
                ["go"] = new() { "google", "gofiber", "godot" },

                // "css" vs "scss"
                ["css"] = new() { "scss", "tailwindcss", "tailwind css" },

                // "node" vs "nodemon", etc. — but "node" is alias for "node.js" so be careful
                // "node" appearing in "nodemon" shouldn't count
                ["node"] = new() { "nodemon" },

                // "angular" vs "angularjs" — these are actually the same skill via alias,
                // so no confusable entry needed.

                // "vue" vs "vuex", "vuetify"
                ["vue"] = new() { "vuex", "vuetify" },

                // "express" vs "expressjs" — same skill via alias, no issue

                // "net" inside ".net", "netlify", "network"
                ["net"] = new() { ".net", "netlify", "network", "netbeans", "networking" },

                // "sql" vs "mysql", "postgresql", "nosql", "sqlite"
                ["sql"] = new() { "mysql", "postgresql", "nosql", "sqlite", "mssql", "sql server", "tsql" },

                // "aws" is unlikely to be confused but "a]" patterns don't apply here

                // "dart" vs "datadog", etc. — unlikely confusable

                // "rest" vs "restful" — same concept, both map to "RESTful APIs" in DB
            };

        public CvTextSkillValidator(ILogger<CvTextSkillValidator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Filters a list of LLM-extracted skill names, keeping only those that
        /// can be verified against the raw CV text.
        /// </summary>
        /// <param name="extractedSkills">Skill names returned by the LLM.</param>
        /// <param name="rawCvText">The raw text extracted from the CV/PDF.</param>
        /// <returns>Filtered list of skills that are present in the CV text.</returns>
        public List<string> ValidateSkills(List<string> extractedSkills, string rawCvText)
        {
            if (extractedSkills == null || extractedSkills.Count == 0)
                return new List<string>();

            if (string.IsNullOrWhiteSpace(rawCvText))
            {
                _logger.LogWarning("[SkillValidator] Raw CV text is empty — skipping validation, returning all skills.");
                return extractedSkills;
            }

            // Normalize CV text once: collapse all whitespace to single spaces, lowercase.
            // This fixes PDF line-break issues like "Java\nScript" → "java script" which
            // still allows matching "javascript" after further normalization.
            var normalizedCv = NormalizeCvText(rawCvText);

            var accepted = new List<string>();
            var dropped = new List<string>();

            foreach (var skill in extractedSkills)
            {
                if (string.IsNullOrWhiteSpace(skill))
                    continue;

                if (IsSkillPresentInCv(skill.Trim(), normalizedCv))
                {
                    accepted.Add(skill);
                }
                else
                {
                    dropped.Add(skill);
                }
            }

            if (dropped.Count > 0)
            {
                _logger.LogWarning(
                    "[SkillValidator] Dropped {Count} hallucinated skill(s): [{Skills}]",
                    dropped.Count, string.Join(", ", dropped));
            }

            _logger.LogInformation(
                "[SkillValidator] Validation result: {Accepted}/{Total} skills accepted",
                accepted.Count, extractedSkills.Count);

            return accepted;
        }

        /// <summary>
        /// Checks whether a single extracted skill name is genuinely present in the CV text.
        /// </summary>
        private bool IsSkillPresentInCv(string extractedSkill, string normalizedCv)
        {
            var normalizedSkill = extractedSkill.ToLowerInvariant().Trim();

            // Tier 1: Confusable-pair check
            if (ConfusablePairs.TryGetValue(normalizedSkill, out var longerForms))
            {
                return IsStandalonePresent(normalizedSkill, longerForms, normalizedCv);
            }

            // Tier 2: Generic presence check (substring match in normalized CV text)
            // We also try a version with whitespace removed for compound names
            // like "node.js" → "nodejs" which might appear as "node.js" or "node js" in CV.
            var skillNoSpaces = Regex.Replace(normalizedSkill, @"[\s.\-/]", "");
            var cvNoSpaces = Regex.Replace(normalizedCv, @"[\s.\-/]", "");

            if (normalizedCv.Contains(normalizedSkill, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (cvNoSpaces.Contains(skillNoSpaces, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Try word-boundary match as a final fallback for short skills
            // that might be missed by simple substring (e.g., skill "R" in text)
            if (normalizedSkill.Length <= 2)
            {
                // For very short skill names (1-2 chars), require word-boundary match
                var pattern = $@"\b{Regex.Escape(normalizedSkill)}\b";
                if (Regex.IsMatch(normalizedCv, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// For a confusable skill (e.g., "java"), checks whether it appears standalone
        /// in the CV text or if every occurrence is actually part of a longer form
        /// (e.g., "javascript").
        ///
        /// Returns true (skill is valid) if at least one occurrence is standalone.
        /// Returns false (hallucination) if ALL occurrences are part of longer forms,
        /// or if the skill doesn't appear at all.
        /// </summary>
        private bool IsStandalonePresent(string shortSkill, List<string> longerForms, string normalizedCv)
        {
            // First check: does the short skill appear at all?
            // Use both normal and stripped versions for compound matches.
            var skillNoSpecial = Regex.Replace(shortSkill, @"[\s.\-/]", "");
            var cvNoSpecial = Regex.Replace(normalizedCv, @"[\s.\-/]", "");

            bool foundInNormal = normalizedCv.Contains(shortSkill, StringComparison.OrdinalIgnoreCase);
            bool foundInStripped = cvNoSpecial.Contains(skillNoSpecial, StringComparison.OrdinalIgnoreCase);

            if (!foundInNormal && !foundInStripped)
            {
                _logger.LogDebug(
                    "[SkillValidator] Confusable skill '{Skill}' not found at all in CV text",
                    shortSkill);
                return false;
            }

            // Second check: find all occurrences and see if they're all within longer forms.
            // We work on the stripped version for reliability.
            var positions = FindAllOccurrences(cvNoSpecial, skillNoSpecial);

            if (positions.Count == 0)
            {
                // Shouldn't happen since we checked above, but be safe
                return false;
            }

            // For each occurrence, check if it's embedded in a longer form
            int embeddedCount = 0;
            foreach (var pos in positions)
            {
                bool isEmbedded = false;
                foreach (var longerForm in longerForms)
                {
                    var longerNorm = Regex.Replace(longerForm.ToLowerInvariant(), @"[\s.\-/]", "");
                    if (IsOccurrencePartOfLongerForm(cvNoSpecial, skillNoSpecial, pos, longerNorm))
                    {
                        isEmbedded = true;
                        break;
                    }
                }

                if (isEmbedded)
                {
                    embeddedCount++;
                }
            }

            if (embeddedCount == positions.Count)
            {
                // ALL occurrences are embedded in longer forms → hallucination
                _logger.LogDebug(
                    "[SkillValidator] Confusable skill '{Skill}': all {Count} occurrences are part of longer forms → rejected",
                    shortSkill, positions.Count);
                return false;
            }

            // At least one standalone occurrence exists
            _logger.LogDebug(
                "[SkillValidator] Confusable skill '{Skill}': {Standalone}/{Total} standalone occurrences → accepted",
                shortSkill, positions.Count - embeddedCount, positions.Count);
            return true;
        }

        /// <summary>
        /// Finds all start indices of <paramref name="needle"/> in <paramref name="haystack"/>.
        /// Case-insensitive.
        /// </summary>
        private static List<int> FindAllOccurrences(string haystack, string needle)
        {
            var results = new List<int>();
            int idx = 0;
            while (idx <= haystack.Length - needle.Length)
            {
                int found = haystack.IndexOf(needle, idx, StringComparison.OrdinalIgnoreCase);
                if (found < 0) break;
                results.Add(found);
                idx = found + 1; // move forward by 1 to find overlapping matches
            }
            return results;
        }

        /// <summary>
        /// Checks if the occurrence of <paramref name="shortSkill"/> at position <paramref name="pos"/>
        /// in <paramref name="text"/> is actually part of the <paramref name="longerForm"/>.
        /// </summary>
        private static bool IsOccurrencePartOfLongerForm(string text, string shortSkill, int pos, string longerForm)
        {
            // Find where the short skill sits within the longer form
            int shortInLonger = longerForm.IndexOf(shortSkill, StringComparison.OrdinalIgnoreCase);
            if (shortInLonger < 0) return false;

            // Calculate where the longer form would start in the text
            int longerStart = pos - shortInLonger;
            if (longerStart < 0) return false;
            if (longerStart + longerForm.Length > text.Length) return false;

            // Check if the longer form actually appears at that position
            return text.Substring(longerStart, longerForm.Length)
                       .Equals(longerForm, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalizes raw CV text by collapsing all whitespace (including \n, \r, \t)
        /// into single spaces and converting to lowercase. This handles PDF extraction
        /// artifacts where words are split across lines.
        /// </summary>
        private static string NormalizeCvText(string rawText)
        {
            // Replace all whitespace sequences (including newlines) with a single space
            var normalized = Regex.Replace(rawText, @"\s+", " ");
            return normalized.ToLowerInvariant().Trim();
        }
    }
}
