using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentPlatformAPI.Models.Identity
{
    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        [StringLength(6)]
        public string OtpCode { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public bool IsUsed { get; set; }
    }
}
