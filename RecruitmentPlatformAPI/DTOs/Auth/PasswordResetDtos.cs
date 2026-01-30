using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Auth
{
/// <summary>
    /// Request to initiate password reset process
    /// </summary>
    public class ForgotPasswordDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format. Please provide a valid email address")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to verify the OTP sent for password reset
    /// </summary>
    public class VerifyResetOtpDto
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
        /// 6-digit OTP code sent to email
        /// </summary>
        /// <example>123456</example>
        [Required(ErrorMessage = "OTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be 6 digits")]
        public string OtpCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to reset password with new password
    /// </summary>
    public class ResetPasswordDto
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
        /// Reset token received from verify-reset-otp endpoint
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; } = string.Empty;

        /// <summary>
        /// New password (minimum 8 characters, must contain uppercase, lowercase, and digit)
        /// </summary>
        /// <example>NewSecurePass123</example>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirm new password (must match new password)
        /// </summary>
        /// <example>NewSecurePass123</example>
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
