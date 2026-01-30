using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Services.Profile;

namespace RecruitmentPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/profile/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        /// <summary>
        /// Add a new project to your profile (Step 2 of profile wizard)
        /// </summary>
        /// <param name="dto">Project details</param>
        /// <returns>Created project with auto-assigned display order</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddProject([FromBody] AddProjectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _projectService.AddProjectAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <param name="projectId">Project ID to update</param>
        /// <param name="dto">Updated project details</param>
        /// <returns>Updated project data</returns>
        [HttpPut("{projectId}")]
        [Authorize]
        [ProducesResponseType(typeof(ProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProject(int projectId, [FromBody] UpdateProjectDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _projectService.UpdateProjectAsync(userId, projectId, dto);

            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a project (soft delete - reorders remaining projects)
        /// </summary>
        /// <param name="projectId">Project ID to delete</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{projectId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _projectService.DeleteProjectAsync(userId, projectId);

            if (!result.Success)
            {
                if (result.Message?.Contains("not found") == true)
                {
                    return NotFound(result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get all your active projects sorted by display order
        /// </summary>
        /// <returns>List of active projects</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<ProjectDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProjects()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var projects = await _projectService.GetProjectsAsync(userId);
            return Ok(new ApiResponse<List<ProjectDto>>(projects!)); // Service always returns non-null list
        }

        // Helper method to extract user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
