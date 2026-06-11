using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Services
{
    /// <summary>
    /// Multi-layer skill matching service. Matches extracted skill names against
    /// the database using a 5-layer pipeline: exact → normalized → alias → word-set → fuzzy.
    /// </summary>
    public class SkillMatcher
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SkillMatcher> _logger;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        // In-memory lookup caches (loaded once, refreshed on demand)
        private Dictionary<string, int>? _exactNameIndex;
        private Dictionary<string, int>? _normalizedIndex;
        private Dictionary<string, int>? _aliasIndex;
        private List<Skill>? _allSkills;
        private DateTime _lastLoaded = DateTime.MinValue;
        private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(10);

        public SkillMatcher(AppDbContext context, ILogger<SkillMatcher> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Matches a list of extracted skill names against the DB.
        /// Returns a list of matched skill IDs (deduplicated, max 25).
        /// </summary>
        public async Task<List<int>> MatchSkillsAsync(List<string> extractedSkillNames)
        {
            await EnsureLoadedAsync();

            var matchedIds = new List<int>();
            var unmatched = new List<string>();
            var matchDetails = new List<string>();

            foreach (var rawName in extractedSkillNames.Take(25))
            {
                if (string.IsNullOrWhiteSpace(rawName)) continue;

                var trimmed = rawName.Trim();
                var (skillId, matchLayer) = await MatchSingleSkillWithLayerAsync(trimmed);

                if (skillId.HasValue && !matchedIds.Contains(skillId.Value))
                {
                    matchedIds.Add(skillId.Value);
                    matchDetails.Add($"'{trimmed}' → ID {skillId.Value} (Layer {matchLayer})");
                }
                else if (!skillId.HasValue)
                {
                    unmatched.Add(trimmed);
                }
            }

            if (matchDetails.Count > 0)
            {
                _logger.LogInformation("Skill matching details: [{Details}]",
                    string.Join(", ", matchDetails));
            }

            if (unmatched.Count > 0)
            {
                _logger.LogWarning("Unmatched skills ({Count}): [{Skills}]",
                    unmatched.Count, string.Join(", ", unmatched));
            }

            _logger.LogInformation("Skill matching summary: {Matched}/{Total} matched",
                matchedIds.Count, extractedSkillNames.Count);

            return matchedIds;
        }

        /// <summary>
        /// Matches a single skill name using the 5-layer pipeline.
        /// Returns the matched skill ID and the layer number that matched.
        /// </summary>
        private async Task<(int? skillId, int layer)> MatchSingleSkillWithLayerAsync(string skillName)
        {
            // Layer 1: Exact match (case-insensitive)
            if (_exactNameIndex != null &&
                _exactNameIndex.TryGetValue(skillName.ToLowerInvariant(), out var exactId))
            {
                _logger.LogDebug("Layer 1 (exact): '{Name}' -> ID {Id}", skillName, exactId);
                return (exactId, 1);
            }

            // Layer 2: Normalized match
            var normalized = NormalizeForSkill(skillName);
            if (!string.IsNullOrEmpty(normalized) && _normalizedIndex != null &&
                _normalizedIndex.TryGetValue(normalized, out var normId))
            {
                _logger.LogDebug("Layer 2 (normalized): '{Name}' ({Norm}) -> ID {Id}",
                    skillName, normalized, normId);
                return (normId, 2);
            }

            // Layer 3: Alias lookup
            if (_aliasIndex != null &&
                _aliasIndex.TryGetValue(skillName.ToLowerInvariant(), out var aliasId))
            {
                _logger.LogDebug("Layer 3 (alias): '{Name}' -> ID {Id}", skillName, aliasId);
                return (aliasId, 3);
            }

            // Layer 4: Word-set match (order-invariant)
            if (_allSkills != null)
            {
                var extractedWords = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (extractedWords.Length > 0)
                {
                    foreach (var skill in _allSkills)
                    {
                        var candidates = new List<string> { skill.Name };
                        if (!string.IsNullOrWhiteSpace(skill.Aliases))
                            candidates.AddRange(skill.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries));

                        foreach (var cand in candidates)
                        {
                            var dbNormalized = NormalizeForSkill(cand);
                            var dbWords = dbNormalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            if (extractedWords.Length == dbWords.Length &&
                                extractedWords.OrderBy(w => w).SequenceEqual(dbWords.OrderBy(w => w)))
                            {
                                _logger.LogDebug("Layer 4 (word-set): '{Name}' -> ID {Id} (via '{Cand}')",
                                    skillName, skill.Id, cand);
                                return (skill.Id, 4);
                            }
                        }
                    }
                }
            }

            // Layer 5: Fuzzy match (Levenshtein, length-scaled threshold)
            if (_allSkills != null)
            {
                var (bestId, matchName, distance) = FindBestFuzzyMatch(normalized, _allSkills);
                
                // Scale allowed distance based on word length to prevent aggressive matching on short words
                int maxAllowedDistance = normalized.Length <= 4 ? 0 :
                                         normalized.Length <= 8 ? 1 : 2;

                if (bestId > 0 && distance <= maxAllowedDistance)
                {
                    _logger.LogDebug("Layer 5 (fuzzy): '{Name}' -> ID {Id} ({MatchName}, distance={Dist})",
                        skillName, bestId, matchName, distance);
                    return (bestId, 5);
                }
            }

            return (null, 0);
        }

        /// <summary>
        /// Loads skills from DB and builds all lookup indices.
        /// </summary>
        private async Task EnsureLoadedAsync()
        {
            if (_allSkills != null && DateTime.UtcNow - _lastLoaded < _cacheTtl)
                return;

            await _loadLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_allSkills != null && DateTime.UtcNow - _lastLoaded < _cacheTtl)
                    return;

                _allSkills = await _context.Skills.ToListAsync();
                _logger.LogInformation("Loaded {Count} skills from DB", _allSkills.Count);

                // Layer 1: Exact name index (lowercase)
                _exactNameIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var skill in _allSkills)
                {
                    var key = skill.Name.ToLowerInvariant().Trim();
                    if (!_exactNameIndex.ContainsKey(key))
                        _exactNameIndex[key] = skill.Id;
                }

                // Layer 2: Normalized name index
                _normalizedIndex = new Dictionary<string, int>();
                foreach (var skill in _allSkills)
                {
                    var norm = NormalizeForSkill(skill.Name);
                    if (!string.IsNullOrEmpty(norm) && !_normalizedIndex.ContainsKey(norm))
                        _normalizedIndex[norm] = skill.Id;

                    // Also add aliases to normalized index for better resilience against formatting
                    if (!string.IsNullOrWhiteSpace(skill.Aliases))
                    {
                        var aliases = skill.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var alias in aliases)
                        {
                            var normAlias = NormalizeForSkill(alias);
                            if (!string.IsNullOrEmpty(normAlias) && !_normalizedIndex.ContainsKey(normAlias))
                                _normalizedIndex[normAlias] = skill.Id;
                        }
                    }
                }

                // Layer 3: Alias index
                _aliasIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var skill in _allSkills)
                {
                    if (string.IsNullOrWhiteSpace(skill.Aliases)) continue;

                    var aliases = skill.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var alias in aliases)
                    {
                        var key = alias.Trim().ToLowerInvariant();
                        if (!string.IsNullOrEmpty(key) && !_aliasIndex.ContainsKey(key))
                            _aliasIndex[key] = skill.Id;
                    }
                }

                _lastLoaded = DateTime.UtcNow;
                _logger.LogInformation("Skill indices built: {Exact} exact, {Norm} normalized, {Alias} aliases",
                    _exactNameIndex.Count, _normalizedIndex.Count, _aliasIndex.Count);
            }
            finally
            {
                _loadLock.Release();
            }
        }

        /// <summary>
        /// Skill-specific normalization: strips dots, spaces, hyphens, slashes.
        /// Keeps #, ++, + as meaningful tokens. Does NOT apply phonetic rules.
        /// </summary>
        private static string NormalizeForSkill(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var normalized = input.ToLowerInvariant().Trim();

            // Remove common prefixes/suffixes that don't affect matching
            normalized = Regex.Replace(normalized, @"\s+", " ");

            // Strip specific non-alphanumeric characters but keep # and +
            // This makes "C#" -> "c#", "C++" -> "c++", "F#" -> "f#"
            // while "ASP.NET Core" -> "aspnetcore", "Node.js" -> "nodejs"
            normalized = Regex.Replace(normalized, @"[.\s\-/]", "");

            return normalized;
        }

        /// <summary>
        /// Finds the best fuzzy match using Levenshtein distance across primary names and aliases.
        /// Returns the best match ID, the specific name/alias that matched, and its distance.
        /// </summary>
        private static (int skillId, string matchName, int distance) FindBestFuzzyMatch(string normalizedInput, List<Skill> skills)
        {
            int bestId = 0;
            string bestName = string.Empty;
            int bestDistance = int.MaxValue;

            foreach (var skill in skills)
            {
                var candidates = new List<string> { skill.Name };
                if (!string.IsNullOrWhiteSpace(skill.Aliases))
                    candidates.AddRange(skill.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries));

                foreach (var cand in candidates)
                {
                    var normalizedDb = NormalizeForSkill(cand);
                    if (string.IsNullOrEmpty(normalizedDb)) continue;

                    var distance = CalculateLevenshteinDistance(normalizedInput, normalizedDb);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestId = skill.Id;
                        bestName = cand;
                    }
                }
            }

            return (bestId, bestName, bestDistance);
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings.
        /// </summary>
        private static int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
            if (string.IsNullOrEmpty(target)) return source.Length;

            var distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; distance[i, 0] = i++) { }
            for (int j = 0; j <= target.Length; distance[0, j] = j++) { }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }
    }
}
