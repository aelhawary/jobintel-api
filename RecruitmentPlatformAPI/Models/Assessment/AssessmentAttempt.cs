using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Reference;
using System.ComponentModel.DataAnnotations;
using JobSeekerModel = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Models.Assessment
{
    /// <summary>
    /// Tracks each assessment quiz attempt
    /// </summary>
    public class AssessmentAttempt
    {
        public int Id { get; set; }
        
        [Required]
        public int JobSeekerId { get; set; }
        
        /// <summary>
        /// Job title at time of assessment (to detect title changes)
        /// </summary>
        [Required]
        public int JobTitleId { get; set; }
        
        /// <summary>
        /// Overall normalized score (0-100)
        /// </summary>
        public decimal? OverallScore { get; set; }
        
        /// <summary>
        /// Technical questions score (0-100), weighted 70% of overall
        /// </summary>
        public decimal? TechnicalScore { get; set; }
        
        /// <summary>
        /// Soft skills questions score (0-100), weighted 30% of overall
        /// </summary>
        public decimal? SoftSkillsScore { get; set; }
        
        [Required]
        public AssessmentStatus Status { get; set; } = AssessmentStatus.InProgress;
        
        [Required]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Time limit in minutes (default: 45)
        /// </summary>
        [Required]
        public int TimeLimitMinutes { get; set; } = 45;
        
        /// <summary>
        /// Calculated: StartedAt + TimeLimitMinutes
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Score validity period (18 months from completion)
        /// </summary>
        public DateTime? ScoreExpiresAt { get; set; }
        
        /// <summary>
        /// Only latest completed attempt is active
        /// </summary>
        public bool IsActive { get; set; } = false;
        
        // Navigation properties
        public JobSeekerModel JobSeeker { get; set; } = null!;
        public JobTitle JobTitle { get; set; } = null!;
        public ICollection<AssessmentAnswer> Answers { get; set; } = new List<AssessmentAnswer>();
        public ICollection<AssessmentSkillScore> SkillScores { get; set; } = new List<AssessmentSkillScore>();
    }
}
