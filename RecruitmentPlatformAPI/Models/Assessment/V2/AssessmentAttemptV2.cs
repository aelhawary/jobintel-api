using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentPlatformAPI.Models.Reference;
using JobSeekerModel = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Models.Assessment.V2
{
    [Table("AssessmentAttemptsV2")]
    public class AssessmentAttemptV2
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobSeekerId { get; set; }

        public int? JobTitleId { get; set; }

        [Required]
        public string QuestionIdsJson { get; set; } = "[]"; // List of IDs from AssessmentQuestionsV2

        [Required]
        public string ClaimedSkillIdsJson { get; set; } = "[]";

        public int TotalQuestions { get; set; }

        public int QuestionsAnswered { get; set; }

        public int ResumeCount { get; set; } = 0;

        public decimal TechnicalScore { get; set; }

        public decimal SoftSkillsScore { get; set; }

        public decimal OverallScore { get; set; }

        public int TimeLimitMinutes { get; set; } = 30;

        public int Status { get; set; } = 1; // 1: InProgress, 2: Completed, 3: Expired

        public int RetakeNumber { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? ScoreExpiresAt { get; set; }

        public virtual ICollection<AssessmentAnswerV2> Answers { get; set; } = new List<AssessmentAnswerV2>();

        // Navigation properties
        public JobSeekerModel? JobSeeker { get; set; }
        public JobTitle? JobTitle { get; set; }
    }
}
