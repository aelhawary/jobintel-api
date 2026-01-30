using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Identity
{
    public class EmailVerification
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(6)]
        public string VerificationCode { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;

        // Navigation property
        public User User { get; set; } = null!;
    }
}
