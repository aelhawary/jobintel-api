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
    /// Request to reset password using the token from email link
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// Secure token from password reset email link
        /// </summary>
        /// <example>a1b2c3d4e5f6...</example>
        [Required(ErrorMessage = "Reset token is required")]
        public string Token { get; set; } = string.Empty;

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

    /// <summary>
    /// Request to validate a password reset token (to check if link is still valid)
    /// </summary>
    public class ValidateResetTokenDto
    {
        /// <summary>
        /// Secure token from password reset email link
        /// </summary>
        [Required(ErrorMessage = "Reset token is required")]
        public string Token { get; set; } = string.Empty;
    }
}
