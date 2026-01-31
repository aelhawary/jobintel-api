using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Services.Profile;

namespace RecruitmentPlatformAPI.Controllers
{
    /// <summary>
    /// Controller for managing work experience entries
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ExperienceController : ControllerBase
    {
        private readonly IExperienceService _experienceService;
        private readonly ILogger<ExperienceController> _logger;

        public ExperienceController(IExperienceService experienceService, ILogger<ExperienceController> logger)
        {
            _experienceService = experienceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all work experience entries for the authenticated user
        /// </summary>
        /// <returns>List of experience entries ordered by display order</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ExperienceListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetExperiences()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _experienceService.GetExperiencesAsync(userId);
            return Ok(new ApiResponse<ExperienceListResponseDto>(result));
        }

        /// <summary>
        /// Get a specific experience entry by ID
        /// </summary>
        /// <param name="id">Experience entry ID</param>
        /// <returns>Experience details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ExperienceResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetExperience(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _experienceService.GetExperienceByIdAsync(userId, id);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("Experience entry not found"));
            }

            return Ok(new ApiResponse<ExperienceResponseDto>(result));
        }

        /// <summary>
        /// Add a new work experience entry
        /// </summary>
        /// <param name="dto">Experience details</param>
        /// <returns>Created experience entry</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ExperienceResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddExperience([FromBody] ExperienceRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _experienceService.AddExperienceAsync(userId, dto);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to add experience. Please check date ranges."));
            }

            _logger.LogInformation("Experience added for user {UserId}: {JobTitle}", userId, dto.JobTitle);
            return CreatedAtAction(nameof(GetExperience), new { id = result.Id }, new ApiResponse<ExperienceResponseDto>(result, "Experience added successfully"));
        }

        /// <summary>
        /// Update an existing work experience entry
        /// </summary>
        /// <param name="id">Experience entry ID</param>
        /// <param name="dto">Updated experience details</param>
        /// <returns>Updated experience entry</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ExperienceResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateExperience(int id, [FromBody] ExperienceRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _experienceService.UpdateExperienceAsync(userId, id, dto);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("Experience entry not found or invalid data"));
            }

            return Ok(new ApiResponse<ExperienceResponseDto>(result, "Experience updated successfully"));
        }

        /// <summary>
        /// Delete a work experience entry (soft delete)
        /// </summary>
        /// <param name="id">Experience entry ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _experienceService.DeleteExperienceAsync(userId, id);
            if (!result)
            {
                return NotFound(new ApiErrorResponse("Experience entry not found"));
            }

            return Ok(new ApiResponse<bool>(true, "Experience deleted successfully"));
        }

        /// <summary>
        /// Reorder experience entries by providing an ordered list of IDs
        /// </summary>
        /// <param name="orderedIds">List of experience IDs in desired order</param>
        /// <returns>Success status</returns>
        [HttpPost("reorder")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReorderExperiences([FromBody] List<int> orderedIds)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            if (orderedIds == null || orderedIds.Count == 0)
            {
                return BadRequest(new ApiErrorResponse("Ordered IDs list is required"));
            }

            var result = await _experienceService.ReorderExperiencesAsync(userId, orderedIds);
            if (!result)
            {
                return BadRequest(new ApiErrorResponse("Failed to reorder experiences"));
            }

            return Ok(new ApiResponse<bool>(true, "Experiences reordered successfully"));
        }

        /// <summary>
        /// Check if user has any experience entries
        /// </summary>
        /// <returns>True if user has at least one experience entry</returns>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> HasExperience()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _experienceService.HasExperienceAsync(userId);
            return Ok(new ApiResponse<bool>(result));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
