using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Services.JobSeeker;

namespace RecruitmentPlatformAPI.Controllers.JobSeeker
{
    /// <summary>
    /// API endpoints for managing job seeker skills (Step 4 of Profile Wizard)
    /// </summary>
    [ApiController]
    [Route("api/jobseeker/skills")]
    [Produces("application/json")]
    [Authorize(Roles = "JobSeeker")]
    public class JobSeekerSkillsController : BaseApiController
    {
        private readonly IJobSeekerSkillService _skillService;
        private readonly ILogger<JobSeekerSkillsController> _logger;

        public JobSeekerSkillsController(IJobSeekerSkillService skillService, ILogger<JobSeekerSkillsController> logger)
        {
            _skillService = skillService;
            _logger = logger;
        }

        /// <summary>
        /// Get all skills assigned to the authenticated job seeker
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<SkillDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSkills()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _skillService.GetSkillsAsync(userId);
            if (!result.Success)
            {
                return BadRequest(new ApiErrorResponse(result.Message));
            }

            return Ok(new ApiResponse<List<SkillDto>>(result.Skills, result.Message));
        }

        /// <summary>
        /// Replace all skills for the job seeker with the provided list
        /// </summary>
        /// <remarks>
        /// This replaces ALL existing skills. Send the complete list of desired skill IDs.
        /// At least one skill is required.
        /// Use GET /api/jobseeker/skills/available to see all available skill options.
        /// </remarks>
        /// <param name="dto">List of skill IDs to assign</param>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<SkillDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateSkills([FromBody] UpdateSkillsRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _skillService.UpdateSkillsAsync(userId, dto);
            if (!result.Success) return BadRequest(new ApiErrorResponse(result.Message));

            return Ok(new ApiResponse<List<SkillDto>>(result.Skills, result.Message));
        }

        /// <summary>
        /// Remove all skills from the job seeker (not allowed)
        /// </summary>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<SkillDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearSkills()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _skillService.ClearSkillsAsync(userId);
            if (!result.Success) return BadRequest(new ApiErrorResponse(result.Message));

            return Ok(new ApiResponse<List<SkillDto>>(result.Skills, result.Message));
        }

        /// <summary>
        /// Get all available skills from the reference table 
        /// </summary>
        /// <remarks>
        /// Returns the full list of skills that can be selected. No authentication required.
        /// </remarks>
        [HttpGet("available")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<SkillDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailableSkills()
        {
            var skills = await _skillService.GetAvailableSkillsAsync();
            return Ok(new ApiResponse<List<SkillDto>>(skills, $"Found {skills.Count} available skills"));
        }
    }
}
