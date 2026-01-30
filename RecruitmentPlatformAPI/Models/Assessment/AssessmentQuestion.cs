using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Reference;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Assessment
{
    /// <summary>
    /// Assessment question for job seeker skill evaluation quizzes
    /// </summary>
    public class AssessmentQuestion
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;
        
        [Required]
        public QuestionCategory Category { get; set; }
        
        /// <summary>
        /// For technical questions only (nullable for soft skills)
        /// </summary>
        public int? SkillId { get; set; }
        
        [Required]
        public QuestionDifficulty Difficulty { get; set; }
        
        [Required]
        public ExperienceSeniorityLevel SeniorityLevel { get; set; }
        
        /// <summary>
        /// JSON array of answer options: ["Option A", "Option B", "Option C", "Option D"]
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Options { get; set; } = string.Empty;
        
        /// <summary>
        /// Index of correct answer (0-3)
        /// </summary>
        [Required]
        public int CorrectAnswerIndex { get; set; }
        
        /// <summary>
        /// Time allowed per question in seconds (default: 60)
        /// </summary>
        public int? TimePerQuestion { get; set; } = 60;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Skill? Skill { get; set; }
    }
}
