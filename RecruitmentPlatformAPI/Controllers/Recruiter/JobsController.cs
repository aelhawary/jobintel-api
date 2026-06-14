using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Services.Recruiter;

namespace RecruitmentPlatformAPI.Controllers.Recruiter
{
    /// <summary>
    /// Job postings management — Recruiter only
    /// </summary>
    [ApiController]
    [Route("api/jobs")]
    [Produces("application/json")]
    [Authorize(Roles = "Recruiter")]
    public class JobsController : BaseApiController
    {
        private readonly IJobService _jobService;
        private readonly ILogger<JobsController> _logger;

        public JobsController(IJobService jobService, ILogger<JobsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        // ══════════════════════════════════════════
        // REFERENCE DATA  (no auth required)
        // ══════════════════════════════════════════

        /// <summary>
        /// Get available skills list for the job creation form
        /// </summary>
        /// <param name="search">Optional: filter by skill name (partial match)</param>
        [HttpGet("skills")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<SkillOptionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSkills([FromQuery] string? search = null)
        {
            var result = await _jobService.GetSkillsAsync(search);
            return Ok(new ApiResponse<List<SkillOptionDto>>(result));
        }

        // ══════════════════════════════════════════
        // JOB MANAGEMENT  (Recruiter only)
        // ══════════════════════════════════════════

        /// <summary>
        /// Get all job postings created by the authenticated recruiter
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10, max: 50)</param>
        /// <param name="isActive">Filter: true = active only, false = inactive only, omit = all</param>
        /// <param name="search">Free-text search filter for job title or company name</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<JobListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyJobs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? search = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var result = await _jobService.GetMyJobsAsync(userId, page, pageSize, isActive, search);
            return MapJobResult(result, data => Ok(new ApiResponse<JobListResponseDto>(data, result.Message)));
        }

        /// <summary>
        /// Get a specific job posting (must be owned by the recruiter)
        /// </summary>
        /// <param name="id">Job ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.GetJobByIdAsync(userId, id);
            return MapJobResult(result, data => Ok(new ApiResponse<JobResponseDto>(data, result.Message)));
        }

        /// <summary>
        /// Create a new job posting (Recruiter only)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateJob([FromBody] JobRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.CreateJobAsync(userId, dto);
            return MapJobResult(result, data => CreatedAtAction(nameof(GetJob), new { id = data.Id },
                new ApiResponse<JobResponseDto>(data, result.Message)));
        }

        /// <summary>
        /// Update an existing job posting (own jobs only)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] JobRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.UpdateJobAsync(userId, id, dto);
            return MapJobResult(result, data => Ok(new ApiResponse<JobResponseDto>(data, result.Message)));
        }

        /// <summary>
        /// Deactivate a job — hides it from candidate matching (own jobs only)
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeactivateJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.DeactivateJobAsync(userId, id);
            return MapJobResult(result, data => Ok(new ApiResponse<bool>(data, result.Message)));
        }

        /// <summary>
        /// Reactivate a previously deactivated job (own jobs only)
        /// </summary>
        [HttpPatch("{id}/reactivate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReactivateJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.ReactivateJobAsync(userId, id);
            return MapJobResult(result, data => Ok(new ApiResponse<bool>(data, result.Message)));
        }

        /// <summary>
        /// Permanently delete a job posting (own jobs only)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.DeleteJobAsync(userId, id);
            return MapJobResult(result, data => Ok(new ApiResponse<bool>(data, result.Message)));
        }


        private IActionResult MapJobResult<T>(JobServiceResult<T> result, Func<T, IActionResult> onSuccess)
        {
            if (result.Success && result.Data != null)
            {
                return onSuccess(result.Data);
            }

            return result.ErrorCode switch
            {
                JobServiceErrorCode.Forbidden => StatusCode(StatusCodes.Status403Forbidden,
                    new ApiErrorResponse(result.Message)),
                JobServiceErrorCode.NotFound => NotFound(new ApiErrorResponse(result.Message)),
                JobServiceErrorCode.ProfileMissing => BadRequest(new ApiErrorResponse(result.Message)),
                JobServiceErrorCode.Validation => BadRequest(new ApiErrorResponse(result.Message)),
                JobServiceErrorCode.ServerError => StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiErrorResponse(result.Message)),
                _ => BadRequest(new ApiErrorResponse(result.Message))
            };
        }
    }
}
