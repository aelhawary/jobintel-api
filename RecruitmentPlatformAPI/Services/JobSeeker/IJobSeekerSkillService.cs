using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public interface IJobSeekerSkillService
    {
        /// <summary>
        /// Get all skills assigned to the job seeker
        /// </summary>
        Task<SkillsResponseDto> GetSkillsAsync(int userId);

        /// <summary>
        /// Replace all skills for the job seeker with the provided skill IDs
        /// </summary>
        Task<SkillsResponseDto> UpdateSkillsAsync(int userId, UpdateSkillsRequestDto dto);

        /// <summary>
        /// Remove all skills from the job seeker (not allowed; at least one skill is required)
        /// </summary>
        Task<SkillsResponseDto> ClearSkillsAsync(int userId);

        /// <summary>
        /// Get all available skills from the reference table
        /// </summary>
        Task<List<SkillDto>> GetAvailableSkillsAsync();
    }
}
