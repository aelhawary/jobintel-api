namespace RecruitmentPlatformAPI.DTOs.Common
{
/// <summary>
    /// Country information with localized name
    /// </summary>
    public class CountryDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Country name (localized based on lang parameter: en/ar)
        /// </summary>
        /// <example>Egypt</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ISO 3166-1 alpha-2 country code
        /// </summary>
        /// <example>EG</example>
        public string IsoCode { get; set; } = string.Empty;

        /// <summary>
        /// International dialing code
        /// </summary>
        /// <example>+20</example>
        public string? PhoneCode { get; set; }
    }
}
