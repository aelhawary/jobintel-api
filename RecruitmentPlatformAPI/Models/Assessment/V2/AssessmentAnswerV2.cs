using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentPlatformAPI.Models.Assessment.V2
{
    [Table("AssessmentAnswersV2")]
    public class AssessmentAnswerV2
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AssessmentAttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public int? SelectedAnswerIndex { get; set; }

        public bool IsCorrect { get; set; }

        public int TimeSpentSeconds { get; set; }

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("AssessmentAttemptId")]
        public virtual AssessmentAttemptV2? AssessmentAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual AssessmentQuestionV2? Question { get; set; }
    }
}
