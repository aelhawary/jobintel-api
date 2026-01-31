using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class Education
    {
        public int Id { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [Required, MaxLength(150)]
        public string Institution { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Degree { get; set; } = string.Empty;
        [Required, MaxLength(150)]
        public string Major { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? GradeOrGPA { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
    }
}
