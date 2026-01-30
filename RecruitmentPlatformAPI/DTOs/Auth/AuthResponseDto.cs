using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.DTOs.Auth
{
/// <summary>
    /// Authentication response containing status, message, token, and user info
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }
        
        /// <summary>
        /// Response message describing the result
        /// </summary>
        /// <example>Login successful.</example>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// JWT authentication token (provided after login or email verification)
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        public string? Token { get; set; }
        
        /// <summary>
        /// Password reset token (provided after OTP verification in password reset flow)
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        public string? ResetToken { get; set; }
        
        /// <summary>
        /// UTC timestamp when account lockout expires (null if not locked)
        /// </summary>
        /// <example>2025-12-05T15:30:00Z</example>
        public DateTime? LockoutEnd { get; set; }
        
        /// <summary>
        /// Remaining minutes until lockout expires
        /// </summary>
        /// <example>15</example>
        public int? RemainingMinutes { get; set; }
        
        /// <summary>
        /// User information (provided after registration, login, or email verification)
        /// </summary>
        public UserInfoDto? User { get; set; }
    }

    /// <summary>
    /// User information returned in authentication responses
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>
        /// User's unique identifier
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }
        
        /// <summary>
        /// User's first name
        /// </summary>
        /// <example>John</example>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's last name
        /// </summary>
        /// <example>Doe</example>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// User's email address
        /// </summary>
        /// <example>john.doe@example.com</example>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Account type: JobSeeker or Recruiter
        /// </summary>
        /// <example>JobSeeker</example>
        public AccountType AccountType { get; set; }
        
        /// <summary>
        /// Indicates if the email address has been verified
        /// </summary>
        /// <example>true</example>
        public bool IsEmailVerified { get; set; }
        
        /// <summary>
        /// Indicates if the account is active
        /// </summary>
        /// <example>true</example>
        public bool IsActive { get; set; }
    }
}
