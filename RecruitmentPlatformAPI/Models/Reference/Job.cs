using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.JobSeeker;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Reference
{
    public class Job
    {
        public int Id { get; set; }
        [Required]
        public int RecruiterId { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [Required, MaxLength(1200)]
        public string Description { get; set; } = string.Empty;
        [Required, MaxLength(1200)]
        public string Requirements { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? EmploymentType { get; set; }
        [Required]
        public int MinYearsOfExperience { get; set; }
        [MaxLength(100)]
        public string? Location { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Recruiter Recruiter { get; set; } = null!;
    }
}
