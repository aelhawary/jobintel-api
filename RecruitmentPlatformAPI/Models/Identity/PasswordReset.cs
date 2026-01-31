using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentPlatformAPI.Models.Identity
{
    /// <summary>
    /// Represents a password reset request with a secure token
    /// </summary>
    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        /// <summary>
        /// Secure random token for password reset link (64 characters)
        /// </summary>
        [Required]
        [StringLength(128)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public bool IsUsed { get; set; }
    }
}
