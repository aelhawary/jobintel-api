using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Recruiter
{
    /// <summary>
    /// Request DTO for creating/updating recruiter company information.
    /// All required fields must be provided. This is the sole step for recruiter profile completion.
    /// </summary>
    public class RecruiterCompanyInfoRequestDto
    {
        /// <summary>
        /// Company or organisation name
        /// </summary>
        /// <example>Acme Corporation</example>
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 150 characters")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Number of employees. Allowed values: 1-10, 11-50, 51-200, 201-500, 501-1000, 1000+
        /// </summary>
        /// <example>51-200</example>
        [Required(ErrorMessage = "Company size is required")]
        [RegularExpression(@"^(1-10|11-50|51-200|201-500|501-1000|1000\+)$",
            ErrorMessage = "Invalid company size. Allowed values: 1-10, 11-50, 51-200, 201-500, 501-1000, 1000+")]
        public string CompanySize { get; set; } = string.Empty;

        /// <summary>
        /// Industry name — must match one of the values returned by GET /api/profile/industries
        /// </summary>
        /// <example>Technology</example>
        [Required(ErrorMessage = "Industry is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Industry must be between 2 and 100 characters")]
        public string Industry { get; set; } = string.Empty;

        /// <summary>
        /// Country ID from reference table
        /// </summary>
        /// <example>65</example>
        [Required(ErrorMessage = "Country is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Valid Country ID is required")]
        public int CountryId { get; set; }

        /// <summary>
        /// City ID from reference table
        /// </summary>
        /// <example>31769</example>
        [Required(ErrorMessage = "City is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Valid City ID is required")]
        public int CityId { get; set; }

        /// <summary>
        /// Company website URL (optional)
        /// </summary>
        /// <example>https://www.acme.com</example>
        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(300, ErrorMessage = "Website URL cannot exceed 300 characters")]
        public string? Website { get; set; }

        /// <summary>
        /// Company LinkedIn page URL (optional)
        /// </summary>
        /// <example>https://www.linkedin.com/company/acme</example>
        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(300, ErrorMessage = "LinkedIn URL cannot exceed 300 characters")]
        public string? LinkedIn { get; set; }

        /// <summary>
        /// Brief description of the company (optional, max 500 characters)
        /// </summary>
        /// <example>Acme Corporation is a leading provider of innovative recruitment solutions.</example>
        [StringLength(500, ErrorMessage = "Company description cannot exceed 500 characters")]
        public string? CompanyDescription { get; set; }
    }

    /// <summary>
    /// Response DTO for recruiter company information (GET).
    /// Extends the request DTO with read-only metadata.
    /// </summary>
    public class RecruiterCompanyInfoDto : RecruiterCompanyInfoRequestDto
    {
        /// <summary>
        /// When the company profile was first created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Name of the Country
        /// </summary>
        public string CountryName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the City
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// When the company profile was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Company logo URL (if uploaded via separate endpoint)
        /// </summary>
        public string? LogoUrl { get; set; }
    }

    /// <summary>
    /// Represents a single industry option for the dropdown
    /// </summary>
    public class IndustryDto
    {
        /// <summary>
        /// Industry name (this value should be sent back in the request)
        /// </summary>
        /// <example>Technology</example>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a single company size option for the dropdown
    /// </summary>
    public class CompanySizeDto
    {
        /// <summary>
        /// Size range label (this exact value should be sent back in the request)
        /// </summary>
        /// <example>51-200</example>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable label for display
        /// </summary>
        /// <example>51-200 employees</example>
        public string Label { get; set; } = string.Empty;
    }
}
