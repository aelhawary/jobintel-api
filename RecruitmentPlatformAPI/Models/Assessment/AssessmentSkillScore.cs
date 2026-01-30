using RecruitmentPlatformAPI.Models.Reference;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Assessment
{
    /// <summary>
    /// Individual skill performance breakdown within an assessment attempt
    /// </summary>
    public class AssessmentSkillScore
    {
        public int Id { get; set; }
        
        [Required]
        public int AssessmentAttemptId { get; set; }
        
        [Required]
        public int SkillId { get; set; }
        
        /// <summary>
        /// Score for this specific skill (0-100)
        /// </summary>
        [Required]
        public decimal Score { get; set; }
        
        /// <summary>
        /// Number of questions for this skill in the assessment
        /// </summary>
        [Required]
        public int QuestionsAttempted { get; set; }
        
        /// <summary>
        /// Number of correct answers for this skill
        /// </summary>
        [Required]
        public int CorrectAnswers { get; set; }
        
        // Navigation properties
        public AssessmentAttempt AssessmentAttempt { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
