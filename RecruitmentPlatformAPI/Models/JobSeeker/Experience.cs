using RecruitmentPlatformAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class Experience
    {
        public int Id { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [Required, MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;
        [Required]
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
        [MaxLength(2000)]
        public string? Responsibilities { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
    }
}
