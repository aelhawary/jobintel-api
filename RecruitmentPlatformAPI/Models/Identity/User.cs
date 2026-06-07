using RecruitmentPlatformAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Identity
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [RegularExpression(@"^[\p{L}\s'-\.]+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [RegularExpression(@"^[\p{L}\s'-\.]+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, apostrophes, and periods")]
        public string LastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format. Please provide a valid email address")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Hashed password - Optional for OAuth users (can be null for Google sign-ins)
        /// </summary>
        public string? PasswordHash { get; set; }
        
        /// <summary>
        /// Authentication provider (Email or Google)
        /// </summary>
        [Required]
        public AuthProvider AuthProvider { get; set; } = AuthProvider.Email;
        
        /// <summary>
        /// Provider's user ID (e.g., Google Sub identifier)
        /// </summary>
        public string? ProviderUserId { get; set; }
        
        /// <summary>
        /// Profile picture URL (from OAuth provider or uploaded)
        /// </summary>
        public string? ProfilePictureUrl { get; set; }
        
        [Required(ErrorMessage = "Account type is required")]
        public AccountType AccountType { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Account Lockout Fields
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAt { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? LockoutReason { get; set; }
        public DateTime? LastSuccessfulLoginAt { get; set; }
        
        /// <summary>
        /// Profile Completion Wizard Progress
        /// 0 = Not started, 1 = Personal Info, 2 = Experience and Education, 
        /// 3 = Projects, 4 = Skills and Social and Certificates (Complete)
        /// </summary>
        public int ProfileCompletionStep { get; set; } = 0;
    }
}
