using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Auth
{
/// <summary>
    /// Email verification request with verification code
    /// </summary>
    public class EmailVerificationDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format. Please provide a valid email address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 6-digit verification code sent to email
        /// </summary>
        /// <example>123456</example>
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be 6 digits")]
        public string VerificationCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to resend email verification code
    /// </summary>
    public class ResendVerificationDto
    {
        /// <summary>
        /// User's email address to resend verification code
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format. Please provide a valid email address")]
        public string Email { get; set; } = string.Empty;
    }
}
