using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Reference
{
    public class JobSkill
    {
        public int Id { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public int SkillId { get; set; }

        // Navigation properties
        public Job Job { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
