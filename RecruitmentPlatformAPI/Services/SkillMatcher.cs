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
        /// Returns a list of matched skill IDs (deduplicated, max 15).
        /// </summary>
        public async Task<List<int>> MatchSkillsAsync(List<string> extractedSkillNames)
        {
            await EnsureLoadedAsync();

            var matchedIds = new List<int>();
            var unmatched = new List<string>();

            foreach (var rawName in extractedSkillNames.Take(15))
            {
                if (string.IsNullOrWhiteSpace(rawName)) continue;

                var trimmed = rawName.Trim();
                var skillId = await MatchSingleSkillAsync(trimmed);

                if (skillId.HasValue && !matchedIds.Contains(skillId.Value))
                {
                    matchedIds.Add(skillId.Value);
                }
                else if (!skillId.HasValue)
                {
                    unmatched.Add(trimmed);
                }
            }

            if (unmatched.Count > 0)
            {
                _logger.LogInformation("Unmatched skills ({Count}): {Skills}",
                    unmatched.Count, string.Join(", ", unmatched));
            }

            _logger.LogInformation("Skill matching: {Matched}/{Total} matched",
                matchedIds.Count, extractedSkillNames.Count);

            return matchedIds;
        }

        /// <summary>
        /// Matches a single skill name using the 5-layer pipeline.
        /// </summary>
        private async Task<int?> MatchSingleSkillAsync(string skillName)
        {
            // Layer 1: Exact match (case-insensitive)
            if (_exactNameIndex != null &&
                _exactNameIndex.TryGetValue(skillName.ToLowerInvariant(), out var exactId))
            {
                _logger.LogDebug("Layer 1 (exact): '{Name}' -> ID {Id}", skillName, exactId);
                return exactId;
            }

            // Layer 2: Normalized match
            var normalized = NormalizeForSkill(skillName);
            if (!string.IsNullOrEmpty(normalized) && _normalizedIndex != null &&
                _normalizedIndex.TryGetValue(normalized, out var normId))
            {
                _logger.LogDebug("Layer 2 (normalized): '{Name}' ({Norm}) -> ID {Id}",
                    skillName, normalized, normId);
                return normId;
            }

            // Layer 3: Alias lookup
            if (_aliasIndex != null &&
                _aliasIndex.TryGetValue(skillName.ToLowerInvariant(), out var aliasId))
            {
                _logger.LogDebug("Layer 3 (alias): '{Name}' -> ID {Id}", skillName, aliasId);
                return aliasId;
            }

            // Layer 4: Word-set match (order-invariant)
            if (_allSkills != null)
            {
                var extractedWords = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (extractedWords.Length > 0)
                {
                    foreach (var skill in _allSkills)
                    {
                        var dbNormalized = NormalizeForSkill(skill.Name);
                        var dbWords = dbNormalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        if (extractedWords.Length == dbWords.Length &&
                            extractedWords.OrderBy(w => w).SequenceEqual(dbWords.OrderBy(w => w)))
                        {
                            _logger.LogDebug("Layer 4 (word-set): '{Name}' -> ID {Id} ({DbName})",
                                skillName, skill.Id, skill.Name);
                            return skill.Id;
                        }
                    }
                }
            }

            // Layer 5: Fuzzy match (Levenshtein, tight threshold)
            if (_allSkills != null)
            {
                var (bestMatch, distance) = FindBestFuzzyMatch(normalized, _allSkills);
                if (bestMatch != null && distance <= 2)
                {
                    _logger.LogDebug("Layer 5 (fuzzy): '{Name}' -> ID {Id} ({DbName}, distance={Dist})",
                        skillName, bestMatch.Id, bestMatch.Name, distance);
                    return bestMatch.Id;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads skills from DB and builds all lookup indices.
        /// </summary>
        private async Task EnsureLoadedAsync()
        {
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
        /// Finds the best fuzzy match using Levenshtein distance.
        /// Returns the best match and its distance.
        /// </summary>
        private static (Skill? skill, int distance) FindBestFuzzyMatch(string normalizedInput, List<Skill> skills)
        {
            Skill? best = null;
            int bestDistance = int.MaxValue;

            foreach (var skill in skills)
            {
                var normalizedDb = NormalizeForSkill(skill.Name);
                var distance = CalculateLevenshteinDistance(normalizedInput, normalizedDb);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = skill;
                }
            }

            return (best, bestDistance);
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
