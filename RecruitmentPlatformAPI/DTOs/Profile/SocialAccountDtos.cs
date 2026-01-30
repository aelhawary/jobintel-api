using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Profile
{
/// <summary>
    /// DTO for updating social account links (Step 6 of profile wizard)
    /// All fields are optional - users can add any combination of links or skip entirely
    /// </summary>
    public class UpdateSocialAccountDto
    {
        [MaxLength(300, ErrorMessage = "LinkedIn URL must not exceed 300 characters")]
        [Url(ErrorMessage = "LinkedIn must be a valid URL")]
        public string? LinkedIn { get; set; }

        [MaxLength(300, ErrorMessage = "GitHub URL must not exceed 300 characters")]
        [Url(ErrorMessage = "GitHub must be a valid URL")]
        public string? Github { get; set; }

        [MaxLength(300, ErrorMessage = "Behance URL must not exceed 300 characters")]
        [Url(ErrorMessage = "Behance must be a valid URL")]
        public string? Behance { get; set; }

        [MaxLength(300, ErrorMessage = "Dribbble URL must not exceed 300 characters")]
        [Url(ErrorMessage = "Dribbble must be a valid URL")]
        public string? Dribbble { get; set; }

        [MaxLength(300, ErrorMessage = "Personal Website URL must not exceed 300 characters")]
        [Url(ErrorMessage = "Personal Website must be a valid URL")]
        public string? PersonalWebsite { get; set; }
    }

    /// <summary>
    /// DTO for social account information
    /// </summary>
    public class SocialAccountDto
    {
        public string? LinkedIn { get; set; }
        public string? Github { get; set; }
        public string? Behance { get; set; }
        public string? Dribbble { get; set; }
        public string? PersonalWebsite { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// API response wrapper for social account operations
    /// </summary>
    public class SocialAccountResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SocialAccountDto? SocialAccounts { get; set; }
    }
}
