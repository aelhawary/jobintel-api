using RecruitmentPlatformAPI.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class Recruiter
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required, MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string CompanySize { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Industry { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Location { get; set; } = string.Empty;
        [MaxLength(300)]
        public string? Website { get; set; }
        [MaxLength(300)]
        public string? LinkedIn { get; set; }
        [MaxLength(500)]
        public string? CompanyDescription { get; set; }
        
        /// <summary>
        /// Company logo URL (different from personal profile picture)
        /// Used for company branding in job posts
        /// Personal profile picture is stored in User.ProfilePictureUrl
        /// </summary>
        [MaxLength(300)]
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
    }
}
