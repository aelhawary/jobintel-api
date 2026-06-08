using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Services.Recruiter;
using RecruitmentPlatformAPI.Services.JobSeeker;

namespace RecruitmentPlatformAPI.Controllers.Recruiter
{
    /// <summary>
    /// Candidate discovery and AI matching — Recruiter only.
    /// Handles browsing matched candidates for a job and recording engagement events.
    /// </summary>
    [ApiController]
    [Route("api/recruiter")]
    [Produces("application/json")]
    [Authorize(Roles = "Recruiter")]
    public class RecruiterCandidatesController : ControllerBase
    {
        private readonly IAIMatchingService _aiMatchingService;
        private readonly IEngagementService _engagementService;
        private readonly AppDbContext _context;
        private readonly ILogger<RecruiterCandidatesController> _logger;

        public RecruiterCandidatesController(
            IAIMatchingService aiMatchingService,
            IEngagementService engagementService,
            AppDbContext context,
            ILogger<RecruiterCandidatesController> logger)
        {
            _aiMatchingService = aiMatchingService;
            _engagementService = engagementService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get AI-matched candidates for a specific job.
        /// Calls the external AI matching engine, stores recommendations,
        /// and records search appearances for all returned candidates.
        /// </summary>
        /// <param name="jobId">The job to find matching candidates for</param>
        /// <param name="maxResults">Maximum number of candidates to return (default: 10, max: 50)</param>
        /// <returns>Ranked list of matched candidates with scores and skill analysis</returns>
        [HttpGet("jobs/{jobId}/candidates")]
        [ProducesResponseType(typeof(ApiResponse<CandidateMatchResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> GetMatchedCandidates(int jobId, [FromQuery] int maxResults = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            // Verify the job belongs to this recruiter
            var recruiter = await _context.Recruiters
                .FirstOrDefaultAsync(r => r.UserId == userId);

                if (recruiter == null)
                    return Forbid();

            var job = await _context.Jobs
                .Include(j => j.JobTitle)
                .FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiter.Id);

            if (job == null)
                return NotFound(new ApiErrorResponse("Job not found or access denied."));

            if (maxResults < 1 || maxResults > 50)
                maxResults = 10;

            // Call the AI matching engine
            var aiResponse = await _aiMatchingService.GetMatchesAsync(jobId, maxResults);

            if (aiResponse == null)
                return StatusCode(StatusCodes.Status502BadGateway,
                    new ApiErrorResponse("AI matching engine is currently unavailable. Please try again later."));

            // Map AI results to our internal DTO with full profile data
            var matchedCandidates = new List<MatchedCandidateDto>();

            foreach (var result in aiResponse.Results)
            {
                if (!int.TryParse(result.CandidateId, out var candidateId))
                    continue;

                var jobSeeker = await _context.JobSeekers
                    .Include(js => js.User)
                    .Include(js => js.JobTitle)
                    .Include(js => js.Country)
                    .Include(js => js.City)
                    .FirstOrDefaultAsync(js => js.Id == candidateId);

                if (jobSeeker == null) continue;

                // Load skills for this candidate
                var skills = await _context.JobSeekerSkills
                    .Where(jss => jss.JobSeekerId == candidateId)
                    .Include(jss => jss.Skill)
                    .Select(jss => jss.Skill.Name)
                    .ToListAsync();

                matchedCandidates.Add(new MatchedCandidateDto
                {
                    JobSeekerId = jobSeeker.Id,
                    FullName = $"{jobSeeker.User.FirstName} {jobSeeker.User.LastName}",
                    ProfilePictureUrl = jobSeeker.User.ProfilePictureUrl,
                    JobTitle = jobSeeker.JobTitle?.TitleEn,
                    Bio = jobSeeker.Bio,
                    YearsOfExperience = jobSeeker.YearsOfExperience,
                    CountryName = jobSeeker.Country?.NameEn,
                    CityName = jobSeeker.City?.NameEn,
                    AssessmentScore = jobSeeker.CurrentAssessmentScore,
                    Skills = skills,
                    MatchScore = result.FinalScore,
                    MatchedSkills = result.MatchedSkills,
                    MissingSkills = result.MissingSkills,
                    AiReasoning = result.Reason
                });
            }

            // Store recommendations in the database
            await _engagementService.StoreRecommendationsAsync(jobId, matchedCandidates);

            // Record search appearances for all returned candidates
            var candidateIds = matchedCandidates.Select(c => c.JobSeekerId).ToList();
            await _engagementService.RecordSearchAppearancesAsync(candidateIds, recruiter.Id, jobId);

            _logger.LogInformation(
                "Recruiter {RecruiterId} viewed {Count} matched candidates for Job {JobId}",
                recruiter.Id, matchedCandidates.Count, jobId);

            return Ok(new ApiResponse<CandidateMatchResponseDto>(new CandidateMatchResponseDto
            {
                JobId = job.Id,
                JobTitle = job.Title,
                JobTitleId = job.JobTitleId,
                JobTitleName = job.JobTitle?.TitleEn,
                TotalPreFiltered = aiResponse.Results.Count,
                TotalMatched = matchedCandidates.Count,
                Candidates = matchedCandidates
            }));
        }

        /// <summary>
        /// Record that the recruiter clicked into a specific candidate's profile.
        /// Call this when the recruiter opens the detailed profile view.
        /// </summary>
        /// <param name="jobId">The job context</param>
        /// <param name="candidateId">The job seeker's ID</param>
        [HttpPost("jobs/{jobId}/candidates/{candidateId}/view")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RecordProfileClick(int jobId, int candidateId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var recruiter = await _context.Recruiters
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter == null)
                return Forbid();

            // Verify the job belongs to this recruiter
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiter.Id);

            if (job == null)
                return NotFound(new ApiErrorResponse("Job not found or access denied."));

            // Verify the candidate exists
            var candidateExists = await _context.JobSeekers.AnyAsync(js => js.Id == candidateId);
            if (!candidateExists)
                return NotFound(new ApiErrorResponse("Candidate not found."));

            // Record the profile click (with 1-hour dedup)
            await _engagementService.RecordProfileViewAsync(candidateId, recruiter.Id, jobId);

            return Ok(new ApiResponse<bool>(true, "Profile view recorded."));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
