using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class SocialAccount
    {
        public int Id { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [MaxLength(300), Url]
        public string? LinkedIn { get; set; }
        [MaxLength(300), Url]
        public string? Github { get; set; }
        [MaxLength(300), Url]
        public string? Behance { get; set; }
        [MaxLength(300), Url]
        public string? Dribbble { get; set; }
        [MaxLength(300), Url]
        public string? PersonalWebsite { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
    }
}
