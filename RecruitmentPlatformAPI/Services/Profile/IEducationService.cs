using RecruitmentPlatformAPI.DTOs.Profile;

namespace RecruitmentPlatformAPI.Services.Profile
{
    /// <summary>
    /// Service interface for managing education entries
    /// </summary>
    public interface IEducationService
    {
        /// <summary>
        /// Get all education entries for a user
        /// </summary>
        Task<EducationListResponseDto> GetEducationAsync(int userId);

        /// <summary>
        /// Get a specific education entry by ID
        /// </summary>
        Task<EducationResponseDto?> GetEducationByIdAsync(int userId, int educationId);

        /// <summary>
        /// Add a new education entry
        /// </summary>
        Task<EducationResponseDto?> AddEducationAsync(int userId, EducationRequestDto dto);

        /// <summary>
        /// Update an existing education entry
        /// </summary>
        Task<EducationResponseDto?> UpdateEducationAsync(int userId, int educationId, EducationRequestDto dto);

        /// <summary>
        /// Delete an education entry (soft delete)
        /// </summary>
        Task<bool> DeleteEducationAsync(int userId, int educationId);

        /// <summary>
        /// Reorder education entries
        /// </summary>
        Task<bool> ReorderEducationAsync(int userId, List<int> orderedIds);

        /// <summary>
        /// Check if user has any education entries
        /// </summary>
        Task<bool> HasEducationAsync(int userId);
    }
}
