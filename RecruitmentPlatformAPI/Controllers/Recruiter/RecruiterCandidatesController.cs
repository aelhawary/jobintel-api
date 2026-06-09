using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
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
    public class RecruiterCandidatesController : BaseApiController
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

            // Batch-load all candidate IDs in 2 queries instead of N+1
            var allCandidateIds = aiResponse.Results
                .Select(r => int.TryParse(r.CandidateId, out var id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var jobSeekersById = await _context.JobSeekers
                .Include(js => js.User)
                .Include(js => js.JobTitle)
                .Include(js => js.Country)
                .Include(js => js.City)
                .Where(js => allCandidateIds.Contains(js.Id))
                .ToDictionaryAsync(js => js.Id);

            var skillsByJobSeekerId = await _context.JobSeekerSkills
                .Where(jss => allCandidateIds.Contains(jss.JobSeekerId))
                .Include(jss => jss.Skill)
                .GroupBy(jss => jss.JobSeekerId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(jss => jss.Skill.Name).ToList());

            foreach (var result in aiResponse.Results)
            {
                if (!int.TryParse(result.CandidateId, out var candidateId))
                    continue;

                if (!jobSeekersById.TryGetValue(candidateId, out var jobSeeker))
                    continue;

                skillsByJobSeekerId.TryGetValue(candidateId, out var skills);

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
                    Skills = skills ?? new List<string>(),
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
        /// Get the full profile of a specific candidate in the context of a job.
        /// Returns personal info, experiences, education, projects, skills, social links,
        /// and assessment score. Records a profile view for engagement tracking.
        /// </summary>
        /// <param name="jobId">The job context (must belong to this recruiter)</param>
        /// <param name="candidateId">The job seeker's ID</param>
        /// <returns>Complete candidate profile</returns>
        [HttpGet("jobs/{jobId}/candidates/{candidateId}")]
        [ProducesResponseType(typeof(ApiResponse<RecruiterCandidateProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCandidateProfile(int jobId, int candidateId)
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

            // Load the candidate with all related data in batch queries
            var jobSeeker = await _context.JobSeekers
                .Include(js => js.User)
                .Include(js => js.JobTitle)
                .Include(js => js.Country)
                .Include(js => js.City)
                .Include(js => js.FirstLanguage)
                .Include(js => js.SecondLanguage)
                .FirstOrDefaultAsync(js => js.Id == candidateId);

            if (jobSeeker == null)
                return NotFound(new ApiErrorResponse("Candidate not found."));

            // Batch-load related entities (3 queries instead of N+1)
            var skills = await _context.JobSeekerSkills
                .Where(jss => jss.JobSeekerId == candidateId)
                .Include(jss => jss.Skill)
                .ToListAsync();

            var experiences = await _context.Experiences
                .Include(e => e.Country)
                .Include(e => e.City)
                .Where(e => e.JobSeekerId == candidateId && !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            var educations = await _context.Educations
                .Include(e => e.FieldOfStudy)
                .Where(e => e.JobSeekerId == candidateId && !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            var projects = await _context.Projects
                .Where(p => p.JobSeekerId == candidateId && !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            var socialAccount = await _context.SocialAccounts
                .FirstOrDefaultAsync(sa => sa.JobSeekerId == candidateId);

            // Record profile click for engagement tracking (1-hour dedup)
            await _engagementService.RecordProfileViewAsync(candidateId, recruiter.Id, jobId);

            var profile = new RecruiterCandidateProfileDto
            {
                JobSeekerId = jobSeeker.Id,
                FirstName = jobSeeker.User.FirstName,
                LastName = jobSeeker.User.LastName,
                Email = jobSeeker.User.Email,
                ProfilePictureUrl = jobSeeker.User.ProfilePictureUrl,
                PhoneNumber = jobSeeker.PhoneNumber,
                Bio = jobSeeker.Bio,

                JobTitleId = jobSeeker.JobTitleId,
                JobTitle = jobSeeker.JobTitle?.TitleEn,
                YearsOfExperience = jobSeeker.YearsOfExperience,

                CountryId = jobSeeker.CountryId,
                Country = jobSeeker.Country?.NameEn,
                CountryCode = jobSeeker.Country?.CountryCode,
                CityId = jobSeeker.CityId,
                City = jobSeeker.City?.NameEn,

                FirstLanguage = jobSeeker.FirstLanguage?.NameEn,
                FirstLanguageProficiency = jobSeeker.FirstLanguageProficiency?.ToString(),
                SecondLanguage = jobSeeker.SecondLanguage?.NameEn,
                SecondLanguageProficiency = jobSeeker.SecondLanguageProficiency?.ToString(),

                WorkPreferences = jobSeeker.WorkPreferences ?? new(),
                DesiredEmploymentTypes = jobSeeker.DesiredEmploymentTypes ?? new(),

                AssessmentScore = jobSeeker.CurrentAssessmentScore,
                LastAssessmentDate = jobSeeker.LastAssessmentDate,

                Skills = skills.Select(s => new RecruiterCandidateSkillDto
                {
                    SkillId = s.SkillId,
                    Name = s.Skill.Name,
                    Source = s.Source
                }).ToList(),

                Experiences = experiences.Select(e => new RecruiterCandidateExperienceDto
                {
                    Id = e.Id,
                    JobTitle = e.JobTitle,
                    CompanyName = e.CompanyName,
                    Country = e.Country?.NameEn,
                    City = e.City?.NameEn,
                    EmploymentType = e.EmploymentType,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Responsibilities = e.Responsibilities,
                    DateRange = FormatDateRange(e.StartDate, e.EndDate, e.IsCurrent)
                }).ToList(),

                Educations = educations.Select(e => new RecruiterCandidateEducationDto
                {
                    Id = e.Id,
                    Institution = e.Institution,
                    Degree = e.Degree,
                    FieldOfStudy = e.FieldOfStudy?.NameEn,
                    GradeOrGPA = e.GradeOrGPA,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    DateRange = FormatDateRange(e.StartDate, e.EndDate, e.IsCurrent)
                }).ToList(),

                Projects = projects.Select(p => new RecruiterCandidateProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    TechnologiesUsed = p.TechnologiesUsed,
                    Description = p.Description,
                    ProjectLink = p.ProjectLink
                }).ToList(),

                SocialAccounts = socialAccount != null ? new RecruiterCandidateSocialDto
                {
                    LinkedIn = socialAccount.LinkedIn,
                    Github = socialAccount.Github,
                    Behance = socialAccount.Behance,
                    Dribbble = socialAccount.Dribbble,
                    PersonalWebsite = socialAccount.PersonalWebsite
                } : null
            };

            _logger.LogInformation(
                "Recruiter {RecruiterId} viewed candidate profile {CandidateId}",
                recruiter.Id, candidateId);

            return Ok(new ApiResponse<RecruiterCandidateProfileDto>(profile));
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

        private static string FormatDateRange(DateTime startDate, DateTime? endDate, bool isCurrent)
        {
            var culture = new System.Globalization.CultureInfo("en-US");
            var start = startDate.ToString("MMM yyyy", culture);
            var end = isCurrent ? "Present" : endDate?.ToString("MMM yyyy", culture) ?? "Present";
            return $"{start} - {end}";
        }

        private static string FormatDateRange(DateTime? startDate, DateTime? endDate, bool isCurrent)
        {
            if (startDate == null && endDate == null && !isCurrent) return "Unknown Date";
            var culture = new System.Globalization.CultureInfo("en-US");
            var start = startDate?.ToString("MMM yyyy", culture) ?? "Unknown";
            var end = isCurrent ? "Present" : endDate?.ToString("MMM yyyy", culture) ?? "Unknown";
            return $"{start} - {end}";
        }
    }
}
