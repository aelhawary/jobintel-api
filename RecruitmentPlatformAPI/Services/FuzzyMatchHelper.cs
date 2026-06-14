using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RecruitmentPlatformAPI.Services
{
    public static class FuzzyMatchHelper
    {
        /// <summary>
        /// Calculates the Levenshtein distance between two strings.
        /// </summary>
        public static int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            int sourceLength = source.Length;
            int targetLength = target.Length;

            var distance = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; distance[i, 0] = i++) { }
            for (int j = 0; j <= targetLength; distance[0, j] = j++) { }

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }

        /// <summary>
        /// Normalizes a string by converting to lowercase, replacing phonetic equivalents,
        /// and removing all non-alphanumeric characters.
        /// e.g. "Fayoum" -> "faiyum", "U.A.E." -> "uae"
        /// </summary>
        public static string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var normalized = input.ToLowerInvariant().Trim();

            // Replace common phonetic equivalents
            normalized = normalized.Replace("ou", "u");
            normalized = normalized.Replace("y", "i");
            normalized = normalized.Replace("ee", "i");
            normalized = normalized.Replace("oo", "u");

            // Remove non-alphanumeric characters (keep only a-z and 0-9)
            normalized = Regex.Replace(normalized, "[^a-z0-9]", "");

            return normalized;
        }

        /// <summary>
        /// Finds the best match for a search string in a collection of items.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="searchStr">The string to search for.</param>
        /// <param name="items">The collection of items.</param>
        /// <param name="stringSelector">A function to extract the string to compare against from an item.</param>
        /// <param name="maxDistance">The maximum acceptable Levenshtein distance.</param>
        /// <returns>The best matching item, or default(T) if no suitable match is found.</returns>
        public static T? FindBestMatch<T>(string searchStr, IEnumerable<T> items, Func<T, string> stringSelector, int maxDistance = 3)
        {
            if (string.IsNullOrWhiteSpace(searchStr) || items == null || !items.Any())
                return default;

            var normalizedSearch = NormalizeString(searchStr);
            if (string.IsNullOrEmpty(normalizedSearch)) return default;

            T? bestMatch = default;
            int minDistance = int.MaxValue;

            foreach (var item in items)
            {
                var itemStr = stringSelector(item);
                if (string.IsNullOrWhiteSpace(itemStr)) continue;

                // Exact match before normalization (case insensitive)
                if (itemStr.Trim().Equals(searchStr.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }

                var normalizedItem = NormalizeString(itemStr);

                // Exact match after normalization
                if (normalizedSearch == normalizedItem)
                {
                    return item;
                }

                var distance = CalculateLevenshteinDistance(normalizedSearch, normalizedItem);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = item;
                }
            }

            // Dynamic threshold: allow larger distance for longer words, but respect the absolute maxDistance
            int dynamicThreshold = Math.Max(maxDistance, normalizedSearch.Length / 3);

            if (minDistance <= dynamicThreshold)
            {
                return bestMatch;
            }

            return default;
        }
    }
}
