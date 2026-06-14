using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Models.Assessment.V2
{
    [Table("AssessmentQuestionsV2")]
    public class AssessmentQuestionV2
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public string Options { get; set; } = "[]"; // JSON array of strings

        [Required]
        public int CorrectAnswerIndex { get; set; }

        public string Explanation { get; set; } = string.Empty;

        public int SkillId { get; set; }

        public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard

        public string Category { get; set; } = "Technical"; // Technical, SoftSkill

        public string? RoleFamily { get; set; }
        
        public string? SeniorityLevel { get; set; }

        public int TimePerQuestion { get; set; } = 60; // seconds

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Skill? Skill { get; set; }
    }
}
