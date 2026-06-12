using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Services.Recruiter;
using System.Security.Claims;

namespace RecruitmentPlatformAPI.Controllers
{
    /// <summary>
    /// Job postings management — Recruiter only
    /// </summary>
    [ApiController]
    [Route("api/jobs")]
    [Produces("application/json")]
    public class JobsController : ControllerBase
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
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<JobListResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyJobs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var result = await _jobService.GetMyJobsAsync(userId, page, pageSize, isActive);
            if (result == null)
                return Forbid();

            return Ok(new ApiResponse<JobListResponseDto>(result));
        }

        /// <summary>
        /// Get a specific job posting (must be owned by the recruiter)
        /// </summary>
        /// <param name="id">Job ID</param>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.GetJobByIdAsync(userId, id);
            if (result == null)
                return NotFound(new ApiErrorResponse("Job not found or you don't have permission to view it"));

            return Ok(new ApiResponse<JobResponseDto>(result));
        }

        /// <summary>
        /// Create a new job posting (Recruiter only)
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateJob([FromBody] JobRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.CreateJobAsync(userId, dto);
            if (result == null)
                return BadRequest(new ApiErrorResponse(
                    "Failed to create job. Ensure you have a Recruiter account and all skill IDs are valid."));

            return CreatedAtAction(nameof(GetJob), new { id = result.Id },
                new ApiResponse<JobResponseDto>(result));
        }

        /// <summary>
        /// Update an existing job posting (own jobs only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<JobResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] JobRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.UpdateJobAsync(userId, id, dto);
            if (result == null)
                return NotFound(new ApiErrorResponse("Job not found or you don't have permission to edit it"));

            return Ok(new ApiResponse<JobResponseDto>(result));
        }

        /// <summary>
        /// Deactivate a job — hides it from candidate matching (own jobs only)
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeactivateJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.DeactivateJobAsync(userId, id);
            if (!result)
                return NotFound(new ApiErrorResponse("Job not found or you don't have permission"));

            return Ok(new ApiResponse<string>("Job deactivated successfully"));
        }

        /// <summary>
        /// Reactivate a previously deactivated job (own jobs only)
        /// </summary>
        [HttpPatch("{id}/reactivate")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReactivateJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.ReactivateJobAsync(userId, id);
            if (!result)
                return NotFound(new ApiErrorResponse("Job not found or you don't have permission"));

            return Ok(new ApiResponse<string>("Job reactivated successfully"));
        }

        /// <summary>
        /// Permanently delete a job posting (own jobs only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.DeleteJobAsync(userId, id);
            if (!result)
                return NotFound(new ApiErrorResponse("Job not found or you don't have permission"));

            return Ok(new ApiResponse<string>("Job deleted successfully"));
        }

        /// <summary>
        /// Get the full profile of a candidate recommended for one of your jobs.
        /// Access is denied if the job doesn't belong to you or if the candidate
        /// was not recommended for this specific job by the AI system.
        /// </summary>
        /// <param name="jobId">Your job ID</param>
        /// <param name="jobSeekerId">The candidate's Job Seeker ID</param>
        [HttpGet("{jobId}/candidates/{jobSeekerId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CandidateProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCandidateProfile(int jobId, int jobSeekerId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _jobService.GetCandidateProfileAsync(userId, jobId, jobSeekerId);

            if (result == null)
                return NotFound(new ApiErrorResponse(
                    "Candidate not found, or they were not recommended for this job, " +
                    "or this job does not belong to you."));

            return Ok(new ApiResponse<CandidateProfileDto>(result));
        }

        // ─── Helper ───────────────────────────────
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Trigger AI recommendations for a specific job.
        /// Pre-filters candidates by skill match then calls the AI API.
        /// </summary>
        /// <param name="jobId">Your job ID</param>
        /// <param name="maxResults">Max number of candidates to return (default: 10)</param>
        [HttpPost("{jobId}/recommendations")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<JobRecommendationsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAiRecommendations(
            int jobId,
            [FromQuery] int maxResults = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            maxResults = Math.Clamp(maxResults, 1, 50);

            var result = await _jobService.GetAiRecommendationsAsync(userId, jobId, maxResults);

            if (result == null)
                return StatusCode(502, new ApiErrorResponse(
                    "Job not found, does not belong to you, or the AI service is unavailable."));

            return Ok(new ApiResponse<JobRecommendationsDto>(result));
        }
    }
}
