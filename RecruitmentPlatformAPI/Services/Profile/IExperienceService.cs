using RecruitmentPlatformAPI.DTOs.Profile;

namespace RecruitmentPlatformAPI.Services.Profile
{
    /// <summary>
    /// Service interface for managing work experience entries
    /// </summary>
    public interface IExperienceService
    {
        /// <summary>
        /// Get all experience entries for a user
        /// </summary>
        Task<ExperienceListResponseDto> GetExperiencesAsync(int userId);

        /// <summary>
        /// Get a specific experience entry by ID
        /// </summary>
        Task<ExperienceResponseDto?> GetExperienceByIdAsync(int userId, int experienceId);

        /// <summary>
        /// Add a new experience entry
        /// </summary>
        Task<ExperienceResponseDto?> AddExperienceAsync(int userId, ExperienceRequestDto dto);

        /// <summary>
        /// Update an existing experience entry
        /// </summary>
        Task<ExperienceResponseDto?> UpdateExperienceAsync(int userId, int experienceId, ExperienceRequestDto dto);

        /// <summary>
        /// Delete an experience entry (soft delete)
        /// </summary>
        Task<bool> DeleteExperienceAsync(int userId, int experienceId);

        /// <summary>
        /// Reorder experience entries
        /// </summary>
        Task<bool> ReorderExperiencesAsync(int userId, List<int> orderedIds);

        /// <summary>
        /// Check if user has any experience entries
        /// </summary>
        Task<bool> HasExperienceAsync(int userId);
    }
}
