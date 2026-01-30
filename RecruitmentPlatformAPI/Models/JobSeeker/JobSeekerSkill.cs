using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Reference;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class JobSeekerSkill
    {
        public int Id { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [Required]
        public int SkillId { get; set; }
        [Required, MaxLength(20)]
        public string Source { get; set; } = "Self";

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
        public Skill Skill { get; set; } = null!;
    }
}
