using RecruitmentPlatformAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Profile
{
/// <summary>
    /// Request DTO for creating/updating personal information (POST/PUT).
    /// Contains only foreign key IDs - localized names are returned in GET responses only.
    /// </summary>
    public class PersonalInfoRequestDto
    {
        /// <summary>
        /// Job title ID from reference table
        /// </summary>
        /// <example>12</example>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Job title is required")]
        public int JobTitleId { get; set; }

        /// <summary>
        /// Total years of professional experience (0-50)
        /// </summary>
        /// <example>3</example>
        [Required]
        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        public int YearsOfExperience { get; set; }

        /// <summary>
        /// Country ID from reference table
        /// </summary>
        /// <example>1</example>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Country is required")]
        public int CountryId { get; set; }

        /// <summary>
        /// City name (auto-normalized to Title Case)
        /// </summary>
        /// <example>Cairo</example>
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Phone number in international E.164 format (e.g., +201234567890)
        /// </summary>
        /// <example>+201234567890</example>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Primary language ID from reference table
        /// </summary>
        /// <example>1</example>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "First language is required")]
        public int FirstLanguageId { get; set; }

        /// <summary>
        /// Proficiency level for first language (Beginner=1, Intermediate=2, Advanced=3, Native=4)
        /// </summary>
        /// <example>Native</example>
        [Required]
        public LanguageProficiency FirstLanguageProficiency { get; set; }

        /// <summary>
        /// Optional secondary language ID from reference table (must differ from FirstLanguageId)
        /// </summary>
        /// <example>2</example>
        [Range(1, int.MaxValue, ErrorMessage = "If provided, second language must be valid")]
        public int? SecondLanguageId { get; set; }

        /// <summary>
        /// Proficiency level for second language (required if SecondLanguageId is provided)
        /// </summary>
        /// <example>Advanced</example>
        public LanguageProficiency? SecondLanguageProficiency { get; set; }
    }

    /// <summary>
    /// Response DTO for personal information (GET).
    /// Extends PersonalInfoRequestDto with localized name properties based on lang parameter (en/ar).
    /// </summary>
    public class PersonalInfoDto : PersonalInfoRequestDto
    {
        /// <summary>
        /// Job title name in English (from JobTitle reference table)
        /// </summary>
        /// <example>Backend Developer</example>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Country name localized based on lang parameter (en/ar)
        /// </summary>
        /// <example>Egypt</example>
        public string? Country { get; set; }

        /// <summary>
        /// First language name localized based on lang parameter (en/ar)
        /// </summary>
        /// <example>Arabic</example>
        public string? FirstLanguage { get; set; }

        /// <summary>
        /// Second language name localized based on lang parameter (en/ar)
        /// </summary>
        /// <example>English</example>
        public string? SecondLanguage { get; set; }
    }
}
