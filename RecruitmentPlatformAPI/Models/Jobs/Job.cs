using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Reference;
using RecruiterEntity = RecruitmentPlatformAPI.Models.Recruiter.Recruiter;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Jobs
{
    public class Job
    {
        public int Id { get; set; }
        [Required]
        public int RecruiterId { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        public int? JobTitleId { get; set; } // FK to JobTitle reference table
        [Required, MaxLength(1200)]
        public string Description { get; set; } = string.Empty;
        [Required, MaxLength(1200)]
        public string Requirements { get; set; } = string.Empty;
        [Required]
        public EmploymentType EmploymentType { get; set; }
        [Required]
        public int MinYearsOfExperience { get; set; }
        [Required]
        public WorkModel WorkModel { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public RecruiterEntity Recruiter { get; set; } = null!;
        public JobTitle? JobTitle { get; set; }
        public Country? Country { get; set; }
        public City? City { get; set; }
        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    }
}
