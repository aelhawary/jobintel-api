using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Auth
{
/// <summary>
    /// DTO for Google OAuth authentication
    /// </summary>
    public class GoogleAuthDto
    {
        /// <summary>
        /// Google ID token received from frontend
        /// </summary>
        /// <example>eyJhbGciOiJSUzI1NiIsImtpZCI6IjE4MmU0...</example>
        [Required(ErrorMessage = "Google ID token is required")]
        public string IdToken { get; set; } = string.Empty;

        /// <summary>
        /// Account type: "JobSeeker" or "Recruiter"
        /// </summary>
        /// <example>JobSeeker</example>
        [Required(ErrorMessage = "Account type is required")]
        [RegularExpression("^(JobSeeker|Recruiter)$", ErrorMessage = "Account type must be either 'JobSeeker' or 'Recruiter'")]
        public string AccountType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Google user information extracted from ID token
    /// </summary>
    public class GoogleUserInfo
    {
        /// <summary>
        /// Google User ID (unique identifier)
        /// </summary>
        public string Sub { get; set; } = string.Empty;
        
        /// <summary>
        /// User's email address from Google
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether the email is verified by Google
        /// </summary>
        public bool EmailVerified { get; set; }
        
        /// <summary>
        /// User's first name from Google
        /// </summary>
        public string GivenName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's last name from Google
        /// </summary>
        public string FamilyName { get; set; } = string.Empty;
        
        /// <summary>
        /// Profile picture URL from Google
        /// </summary>
        public string? Picture { get; set; }
    }
}
