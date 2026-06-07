using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class Project
    {
        public int Id { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(300)]
        public string? TechnologiesUsed { get; set; }
        [MaxLength(1200)]
        public string? Description { get; set; }
        [MaxLength(300)]
        public string? ProjectLink { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
    }
}
