using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public interface IProjectService
    {
        /// <summary>
        /// Add a new project for the authenticated job seeker
        /// </summary>
        Task<ProjectResponseDto> AddProjectAsync(int userId, AddProjectDto dto);

        /// <summary>
        /// Update an existing project
        /// </summary>
        Task<ProjectResponseDto> UpdateProjectAsync(int userId, int projectId, UpdateProjectDto dto);

        /// <summary>
        /// Soft delete a project and reorder remaining projects
        /// </summary>
        Task<ApiResponse<bool>> DeleteProjectAsync(int userId, int projectId);

        /// <summary>
        /// Get all active projects for the authenticated job seeker
        /// </summary>
        Task<List<ProjectDto>> GetProjectsAsync(int userId);
    }
}
