using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Services.Profile;

namespace RecruitmentPlatformAPI.Controllers
{
    /// <summary>
    /// Controller for managing education entries
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class EducationController : ControllerBase
    {
        private readonly IEducationService _educationService;
        private readonly ILogger<EducationController> _logger;

        public EducationController(IEducationService educationService, ILogger<EducationController> logger)
        {
            _educationService = educationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all education entries for the authenticated user
        /// </summary>
        /// <returns>List of education entries ordered by display order</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<EducationListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEducation()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _educationService.GetEducationAsync(userId);
            return Ok(new ApiResponse<EducationListResponseDto>(result));
        }

        /// <summary>
        /// Get a specific education entry by ID
        /// </summary>
        /// <param name="id">Education entry ID</param>
        /// <returns>Education details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EducationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEducationById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _educationService.GetEducationByIdAsync(userId, id);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("Education entry not found"));
            }

            return Ok(new ApiResponse<EducationResponseDto>(result));
        }

        /// <summary>
        /// Add a new education entry
        /// </summary>
        /// <param name="dto">Education details</param>
        /// <returns>Created education entry</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EducationResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddEducation([FromBody] EducationRequestDto dto)
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

            var result = await _educationService.AddEducationAsync(userId, dto);
            if (result == null)
            {
                return BadRequest(new ApiErrorResponse("Failed to add education. Please check date ranges."));
            }

            _logger.LogInformation("Education added for user {UserId}: {Degree} at {Institution}", userId, dto.Degree, dto.Institution);
            return CreatedAtAction(nameof(GetEducationById), new { id = result.Id }, new ApiResponse<EducationResponseDto>(result, "Education added successfully"));
        }

        /// <summary>
        /// Update an existing education entry
        /// </summary>
        /// <param name="id">Education entry ID</param>
        /// <param name="dto">Updated education details</param>
        /// <returns>Updated education entry</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<EducationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateEducation(int id, [FromBody] EducationRequestDto dto)
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

            var result = await _educationService.UpdateEducationAsync(userId, id, dto);
            if (result == null)
            {
                return NotFound(new ApiErrorResponse("Education entry not found or invalid data"));
            }

            return Ok(new ApiResponse<EducationResponseDto>(result, "Education updated successfully"));
        }

        /// <summary>
        /// Delete an education entry (soft delete)
        /// </summary>
        /// <param name="id">Education entry ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _educationService.DeleteEducationAsync(userId, id);
            if (!result)
            {
                return NotFound(new ApiErrorResponse("Education entry not found"));
            }

            return Ok(new ApiResponse<bool>(true, "Education deleted successfully"));
        }

        /// <summary>
        /// Reorder education entries by providing an ordered list of IDs
        /// </summary>
        /// <param name="orderedIds">List of education IDs in desired order</param>
        /// <returns>Success status</returns>
        [HttpPost("reorder")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReorderEducation([FromBody] List<int> orderedIds)
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

            var result = await _educationService.ReorderEducationAsync(userId, orderedIds);
            if (!result)
            {
                return BadRequest(new ApiErrorResponse("Failed to reorder education entries"));
            }

            return Ok(new ApiResponse<bool>(true, "Education entries reordered successfully"));
        }

        /// <summary>
        /// Check if user has any education entries
        /// </summary>
        /// <returns>True if user has at least one education entry</returns>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> HasEducation()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _educationService.HasEducationAsync(userId);
            return Ok(new ApiResponse<bool>(result));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
