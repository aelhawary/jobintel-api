using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Auth
{
/// <summary>
    /// Login credentials for user authentication
    /// </summary>
    public class LoginDto
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
        /// User's password
        /// </summary>
        /// <example>SecurePass123!</example>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
