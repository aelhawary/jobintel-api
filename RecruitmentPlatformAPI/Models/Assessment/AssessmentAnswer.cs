using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Assessment
{
    /// <summary>
    /// Individual answer for each question in an assessment attempt
    /// </summary>
    public class AssessmentAnswer
    {
        public int Id { get; set; }
        
        [Required]
        public int AssessmentAttemptId { get; set; }
        
        [Required]
        public int QuestionId { get; set; }
        
        /// <summary>
        /// Selected answer index (0-3)
        /// </summary>
        [Required]
        public int SelectedAnswerIndex { get; set; }
        
        [Required]
        public bool IsCorrect { get; set; }
        
        [Required]
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public AssessmentAttempt AssessmentAttempt { get; set; } = null!;
        public AssessmentQuestion Question { get; set; } = null!;
    }
}
