using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.Reference;
using RecruitmentPlatformAPI.Models.Assessment;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class JobSeeker
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public int? JobTitleId { get; set; } // Foreign key to JobTitle table
        public int? YearsOfExperience { get; set; }
        public int? CountryId { get; set; } // Foreign key to Country table
        [MaxLength(100)]
        public string? City { get; set; }
        [Phone, MaxLength(20)]
        public string? PhoneNumber { get; set; }
        public int? FirstLanguageId { get; set; } // Foreign key to Language table
        public LanguageProficiency? FirstLanguageProficiency { get; set; }
        public int? SecondLanguageId { get; set; } // Foreign key to Language table
        public LanguageProficiency? SecondLanguageProficiency { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Assessment-related fields
        /// <summary>
        /// Current active assessment score (denormalized for quick MatchScore calculation)
        /// </summary>
        public decimal? CurrentAssessmentScore { get; set; }
        
        /// <summary>
        /// Date of last completed assessment (for 60-day cooldown enforcement)
        /// </summary>
        public DateTime? LastAssessmentDate { get; set; }
        
        /// <summary>
        /// Job title when last assessed (to detect role family changes)
        /// </summary>
        public int? AssessmentJobTitleId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public Country? Country { get; set; }
        public JobTitle? JobTitle { get; set; }
        public Language? FirstLanguage { get; set; }
        public Language? SecondLanguage { get; set; }
        public ICollection<AssessmentAttempt> AssessmentAttempts { get; set; } = new List<AssessmentAttempt>();
        
        // Note: ProfilePictureUrl is now stored in User table only (single source of truth)
    }
}
