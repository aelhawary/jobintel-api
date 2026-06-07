using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Auth
{
/// <summary>
    /// Registration request containing user details and account type
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// User's first name
        /// </summary>
        /// <example>John</example>
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [RegularExpression(@"^[\p{L}\s'-\.]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        /// <example>Doe</example>
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [RegularExpression(@"^[\p{L}\s'-\.]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format. Please provide a valid email address")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password (minimum 8 characters, must contain uppercase, lowercase, and digit)
        /// </summary>
        /// <example>SecurePass123</example>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Password confirmation (must match password)
        /// </summary>
        /// <example>SecurePass123</example>
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Account type: "JobSeeker" or "Recruiter"
        /// </summary>
        /// <example>JobSeeker</example>
        [Required(ErrorMessage = "Account type is required")]
        public string AccountType { get; set; } = string.Empty; // "JobSeeker" or "Recruiter"
    }
}
