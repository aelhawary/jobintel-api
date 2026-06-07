using System.ComponentModel.DataAnnotations;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Models.Jobs
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
