using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Request DTO for replacing a job seeker's skills
    /// </summary>
    public class UpdateSkillsRequestDto
    {
        /// <summary>
        /// List of skill IDs to assign to the job seeker (replaces all existing)
        /// </summary>
        /// <example>[1, 5, 12, 23]</example>
        [Required(ErrorMessage = "At least one skill must be selected")]
        [MinLength(1, ErrorMessage = "At least one skill must be selected")]
        [MaxLength(25, ErrorMessage = "Maximum 25 skills allowed.")]
        public List<int> SkillIds { get; set; } = new();
    }

    /// <summary>
    /// DTO for a single skill
    /// </summary>
    public class SkillDto
    {
        /// <summary>
        /// Skill ID
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Skill name
        /// </summary>
        /// <example>C#</example>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response wrapper for skill operations
    /// </summary>
    public class SkillsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<SkillDto> Skills { get; set; } = new();
        public int TotalCount { get; set; }

        public static SkillsResponseDto SuccessResult(List<SkillDto> skills, string message = "Operation successful")
        {
            return new SkillsResponseDto { Success = true, Message = message, Skills = skills, TotalCount = skills.Count };
        }

        public static SkillsResponseDto FailureResult(string message)
        {
            return new SkillsResponseDto { Success = false, Message = message };
        }
    }
}
