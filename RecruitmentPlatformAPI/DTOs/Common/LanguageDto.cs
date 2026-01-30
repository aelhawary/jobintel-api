namespace RecruitmentPlatformAPI.DTOs.Common
{
/// <summary>
    /// Language information with localized name
    /// </summary>
    public class LanguageDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Language name (localized based on lang parameter: en/ar)
        /// </summary>
        /// <example>Arabic</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ISO 639-3 language code
        /// </summary>
        /// <example>ara</example>
        public string IsoCode { get; set; } = string.Empty;
    }
}
