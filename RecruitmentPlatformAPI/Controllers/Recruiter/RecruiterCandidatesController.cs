using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Services.Recruiter;
using RecruitmentPlatformAPI.Services.JobSeeker;
using RecruitmentPlatformAPI.Configuration;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Models.Recruiter;
using RecruitmentPlatformAPI.Models.Reference;

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
        // Default profile picture — relative path served by the API static files middleware
        private const string DefaultPictureRelativePath = "/images/default-profile.png";
        private readonly string _defaultProfilePictureUrl;

        private readonly IAIMatchingService _aiMatchingService;
        private readonly IEngagementService _engagementService;
        private readonly AppDbContext _context;
        private readonly ILogger<RecruiterCandidatesController> _logger;
        private readonly FileStorageSettings _fileSettings;

        public RecruiterCandidatesController(
            IAIMatchingService aiMatchingService,
            IEngagementService engagementService,
            AppDbContext context,
            ILogger<RecruiterCandidatesController> logger,
            IOptions<FileStorageSettings> fileSettings)
        {
            _aiMatchingService = aiMatchingService;
            _engagementService = engagementService;
            _context = context;
            _logger = logger;
            _fileSettings = fileSettings.Value;
            _defaultProfilePictureUrl = $"{_fileSettings.BaseUrl}{DefaultPictureRelativePath}";
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

            var lang = GetLanguage(HttpContext);

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

            // ── Fallback: if AI API is unavailable, load from Recommendations table ──
            if (aiResponse == null)
            {
                _logger.LogWarning(
                    "AI matching engine unavailable for Job {JobId}. Falling back to Recommendations table.",
                    jobId);

                var recommendations = await _context.Recommendations
                    .Where(r => r.JobId == jobId)
                    .Include(r => r.JobSeeker)
                        .ThenInclude(js => js.User)
                    .Include(r => r.JobSeeker)
                        .ThenInclude(js => js.JobTitle)
                    .Include(r => r.JobSeeker)
                        .ThenInclude(js => js.Country)
                    .Include(r => r.JobSeeker)
                        .ThenInclude(js => js.City)
                    .OrderByDescending(r => r.MatchScore)
                    .Take(maxResults)
                    .ToListAsync();

                // No cached recommendations for a brand-new job: return a friendly empty state
                // instead of a hard 502, so the UI shows "Recommendations are being generated"
                // rather than a generic error screen.
                if (!recommendations.Any())
                {
                    _logger.LogInformation(
                        "No cached recommendations for Job {JobId}. AI is unavailable and no fallback data exists.",
                        jobId);

                    return Ok(new ApiResponse<CandidateMatchResponseDto>(new CandidateMatchResponseDto
                    {
                        JobId = job.Id,
                        JobTitle = job.Title,
                        JobTitleId = job.JobTitleId,
                        JobTitleName = job.JobTitle?.TitleEn,
                        TotalPreFiltered = 0,
                        TotalMatched = 0,
                        Candidates = new List<MatchedCandidateDto>()
                    }, "AI matching engine is temporarily unavailable. Recommendations will appear once the service recovers."));
                }

                var recCandidateIds = recommendations.Select(r => r.JobSeekerId).ToList();
                var recSkills = await _context.JobSeekerSkills
                    .Where(jss => recCandidateIds.Contains(jss.JobSeekerId))
                    .Include(jss => jss.Skill)
                    .GroupBy(jss => jss.JobSeekerId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(jss => jss.Skill.Name).ToList());

                var shortlistedIds = await _context.ShortlistedCandidates
                    .Where(sc => sc.JobId == jobId && sc.RecruiterId == recruiter.Id && recCandidateIds.Contains(sc.JobSeekerId))
                    .Select(sc => sc.JobSeekerId)
                    .ToListAsync();

                var fallbackCandidates = recommendations.Select(rec =>
                {
                    var js = rec.JobSeeker;
                    recSkills.TryGetValue(js.Id, out var skills);
                    return MapToMatchedCandidateDto(js, rec, skills, shortlistedIds.Contains(js.Id), _defaultProfilePictureUrl, lang);
                }).ToList();

                // Record search appearances for fallback candidates
                var fallbackCandidateIds = fallbackCandidates.Select(c => c.JobSeekerId).ToList();
                await _engagementService.RecordSearchAppearancesAsync(fallbackCandidateIds, recruiter.Id, jobId);

                _logger.LogInformation(
                    "Recruiter {RecruiterId} viewed {Count} candidates (fallback) for Job {JobId}",
                    recruiter.Id, fallbackCandidates.Count, jobId);

                return Ok(new ApiResponse<CandidateMatchResponseDto>(new CandidateMatchResponseDto
                {
                    JobId = job.Id,
                    JobTitle = job.Title,
                    JobTitleId = job.JobTitleId,
                    JobTitleName = job.JobTitle?.TitleEn,
                    TotalPreFiltered = recommendations.Count,
                    TotalMatched = fallbackCandidates.Count,
                    Candidates = fallbackCandidates
                }));
            }

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

            var aiShortlistedIds = await _context.ShortlistedCandidates
                .Where(sc => sc.JobId == jobId && sc.RecruiterId == recruiter.Id && allCandidateIds.Contains(sc.JobSeekerId))
                .Select(sc => sc.JobSeekerId)
                .ToListAsync();

            foreach (var result in aiResponse.Results)
            {
                if (!int.TryParse(result.CandidateId, out var candidateId))
                    continue;

                if (!jobSeekersById.TryGetValue(candidateId, out var jobSeeker))
                    continue;

                skillsByJobSeekerId.TryGetValue(candidateId, out var skills);

                var dto = MapToMatchedCandidateDto(jobSeeker, null, skills, aiShortlistedIds.Contains(jobSeeker.Id), _defaultProfilePictureUrl, lang);
                
                // Override match data with AI-computed values
                var aiScore = result.FinalScore;
                
                // Fairness Calculation: Apply a 20% penalty for unassessed candidates
                // This ensures they don't unfairly outrank candidates who actually took the test.
                if (jobSeeker.CurrentAssessmentScore == null)
                {
                    aiScore = Math.Round(aiScore * 0.8m, 2);
                }
                
                dto.MatchScore = aiScore;
                dto.MatchedSkills = result.MatchedSkills;
                dto.MissingSkills = result.MissingSkills;
                dto.AiReasoning = result.Reason;
                matchedCandidates.Add(dto);
            }

            // Re-sort the candidates descending by our newly adjusted MatchScore
            matchedCandidates = matchedCandidates.OrderByDescending(c => c.MatchScore).ToList();


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

            var lang = GetLanguage(HttpContext);

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
                .AsNoTracking()
                .Include(js => js.User)
                .Include(js => js.JobTitle)
                .Include(js => js.Country)
                .Include(js => js.City)
                .Include(js => js.FirstLanguage)
                .Include(js => js.SecondLanguage)
                .AsSplitQuery()
                .FirstOrDefaultAsync(js => js.Id == candidateId);

            if (jobSeeker == null)
                return NotFound(new ApiErrorResponse("Candidate not found."));

            // Batch-load related entities (5 queries instead of N+1)
            var skills = await _context.JobSeekerSkills
                .AsNoTracking()
                .Where(jss => jss.JobSeekerId == candidateId)
                .Include(jss => jss.Skill)
                .ToListAsync();

            var experiences = await _context.Experiences
                .AsNoTracking()
                .Include(e => e.Country)
                .Include(e => e.City)
                .Where(e => e.JobSeekerId == candidateId && !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            var educations = await _context.Educations
                .AsNoTracking()
                .Include(e => e.FieldOfStudy)
                .Where(e => e.JobSeekerId == candidateId && !e.IsDeleted)
                .OrderBy(e => e.DisplayOrder)
                .ThenByDescending(e => e.StartDate)
                .ToListAsync();

            var projects = await _context.Projects
                .AsNoTracking()
                .Where(p => p.JobSeekerId == candidateId && !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            var socialAccount = await _context.SocialAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(sa => sa.JobSeekerId == candidateId);

            // Load resume (latest uploaded)
            var resume = await _context.Resumes
                .AsNoTracking()
                .Where(r => r.JobSeekerId == candidateId)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            // Load AI match recommendation for this specific job
            var recommendation = await _context.Recommendations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.JobId == jobId && r.JobSeekerId == candidateId);

            var profile = new RecruiterCandidateProfileDto
            {
                JobSeekerId = jobSeeker.Id,
                FirstName = jobSeeker.User.FirstName,
                LastName = jobSeeker.User.LastName,
                Email = jobSeeker.User.Email,
                ProfilePictureUrl = jobSeeker.User.ProfilePictureUrl ?? _defaultProfilePictureUrl,
                PhoneNumber = jobSeeker.PhoneNumber,
                Bio = jobSeeker.Bio,

                JobTitleId = jobSeeker.JobTitleId,
                JobTitle = LocalizeTitle(jobSeeker.JobTitle?.TitleEn, jobSeeker.JobTitle?.TitleAr, lang),
                YearsOfExperience = jobSeeker.YearsOfExperience,

                CountryId = jobSeeker.CountryId,
                Country = Localize(jobSeeker.Country?.NameEn, jobSeeker.Country?.NameAr, lang),
                CountryCode = jobSeeker.Country?.CountryCode,
                CityId = jobSeeker.CityId,
                City = Localize(jobSeeker.City?.NameEn, jobSeeker.City?.NameAr, lang),

                FirstLanguage = Localize(jobSeeker.FirstLanguage?.NameEn, jobSeeker.FirstLanguage?.NameAr, lang),
                FirstLanguageProficiency = jobSeeker.FirstLanguageProficiency?.ToString(),
                SecondLanguage = Localize(jobSeeker.SecondLanguage?.NameEn, jobSeeker.SecondLanguage?.NameAr, lang),
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
                    Country = Localize(e.Country?.NameEn, e.Country?.NameAr, lang),
                    City = Localize(e.City?.NameEn, e.City?.NameAr, lang),
                    EmploymentType = e.EmploymentType,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    Responsibilities = e.Responsibilities,
                    DateRange = FormatDateRange(e.StartDate, e.EndDate, e.IsCurrent, lang)
                }).ToList(),

                Educations = educations.Select(e => new RecruiterCandidateEducationDto
                {
                    Id = e.Id,
                    Institution = e.Institution,
                    Degree = e.Degree,
                    FieldOfStudy = Localize(e.FieldOfStudy?.NameEn, e.FieldOfStudy?.NameAr, lang) ?? e.FieldOfStudyName,
                    GradeOrGPA = e.GradeOrGPA,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent,
                    DateRange = FormatDateRange(e.StartDate, e.EndDate, e.IsCurrent, lang)
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
                } : null,

                // Resume
                ResumeFileName = resume?.FileName,
                ResumeFilePath = resume != null ? $"{_fileSettings.BaseUrl}/Uploads/{resume.FilePath.Replace('\\', '/')}" : null,
                ResumeFileSizeBytes = resume?.FileSizeBytes,

                // AI Match
                MatchScore = recommendation?.MatchScore,
                MatchedSkills = !string.IsNullOrEmpty(recommendation?.MatchedSkillsJson)
                    ? JsonSerializer.Deserialize<List<string>>(recommendation.MatchedSkillsJson) ?? new()
                    : new(),
                MissingSkills = !string.IsNullOrEmpty(recommendation?.MissingSkillsJson)
                    ? JsonSerializer.Deserialize<List<string>>(recommendation.MissingSkillsJson) ?? new()
                    : new(),
                AiReasoning = recommendation?.AiReasoning,
                IsShortlisted = await _context.ShortlistedCandidates.AnyAsync(sc => sc.JobId == jobId && sc.JobSeekerId == candidateId && sc.RecruiterId == recruiter.Id)
            };

            _logger.LogInformation(
                "Recruiter {RecruiterId} viewed candidate profile {CandidateId}",
                recruiter.Id, candidateId);

            return Ok(new ApiResponse<RecruiterCandidateProfileDto>(profile));
        }

        [HttpGet("jobs/{jobId}/candidates/shortlisted")]
        [ProducesResponseType(typeof(ApiResponse<List<MatchedCandidateDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetShortlistedCandidates(int jobId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var lang = GetLanguage(HttpContext);

            var recruiter = await _context.Recruiters
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter == null)
                return Forbid();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiter.Id);

            if (job == null)
                return NotFound(new ApiErrorResponse("Job not found or access denied."));

            var shortlistedCandidateIds = await _context.ShortlistedCandidates
                .Where(sc => sc.JobId == jobId && sc.RecruiterId == recruiter.Id)
                .Select(sc => sc.JobSeekerId)
                .ToListAsync();

            if (!shortlistedCandidateIds.Any())
                return Ok(new ApiResponse<List<MatchedCandidateDto>>(new List<MatchedCandidateDto>()));

            var jobSeekers = await _context.JobSeekers
                .AsNoTracking()
                .Include(js => js.User)
                .Include(js => js.JobTitle)
                .Include(js => js.Country)
                .Include(js => js.City)
                .Where(js => shortlistedCandidateIds.Contains(js.Id))
                .ToListAsync();

            var skillsByJobSeekerId = await _context.JobSeekerSkills
                .Where(jss => shortlistedCandidateIds.Contains(jss.JobSeekerId))
                .Include(jss => jss.Skill)
                .GroupBy(jss => jss.JobSeekerId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(jss => jss.Skill.Name).ToList());

            var recommendations = await _context.Recommendations
                .Where(r => r.JobId == jobId && shortlistedCandidateIds.Contains(r.JobSeekerId))
                .ToDictionaryAsync(r => r.JobSeekerId);

            var matchedCandidates = new List<MatchedCandidateDto>();

            foreach (var js in jobSeekers)
            {
                skillsByJobSeekerId.TryGetValue(js.Id, out var skills);
                recommendations.TryGetValue(js.Id, out var rec);
                matchedCandidates.Add(MapToMatchedCandidateDto(js, rec, skills, isShortlisted: true, _defaultProfilePictureUrl, lang));
            }

            return Ok(new ApiResponse<List<MatchedCandidateDto>>(matchedCandidates));
        }

        [HttpPost("jobs/{jobId}/candidates/{candidateId}/toggle-shortlist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleShortlist(int jobId, int candidateId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var recruiter = await _context.Recruiters
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (recruiter == null)
                return Forbid();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiter.Id);

            if (job == null)
                return NotFound(new ApiErrorResponse("Job not found or access denied."));

            var jobSeekerExists = await _context.JobSeekers.AnyAsync(js => js.Id == candidateId);
            if (!jobSeekerExists)
                return NotFound(new ApiErrorResponse("Candidate not found."));

            var existingShortlist = await _context.ShortlistedCandidates
                .FirstOrDefaultAsync(sc => sc.JobId == jobId && sc.JobSeekerId == candidateId && sc.RecruiterId == recruiter.Id);

            bool isShortlisted = false;

            if (existingShortlist != null)
            {
                _context.ShortlistedCandidates.Remove(existingShortlist);
            }
            else
            {
                _context.ShortlistedCandidates.Add(new ShortlistedCandidate
                {
                    JobId = jobId,
                    JobSeekerId = candidateId,
                    RecruiterId = recruiter.Id,
                    ShortlistedAt = DateTime.UtcNow
                });
                isShortlisted = true;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Concurrency or unique constraint violation.
                // It means the state was already changed by a concurrent request.
                // We return Ok because the desired end state matches what's in the DB.
            }

            return Ok(new ApiResponse<bool>(isShortlisted, isShortlisted ? "Candidate shortlisted successfully." : "Candidate removed from shortlist."));
        }

        // ── Helper Methods ──

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

        /// <summary>
        /// Download a candidate's resume directly
        /// </summary>
        [HttpGet("jobs/{jobId}/candidates/{candidateId}/resume/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadCandidateResume(int jobId, int candidateId)
        {
            var userId = GetCurrentUserId();
            var recruiter = await _context.Recruiters.FirstOrDefaultAsync(r => r.UserId == userId);
            if (recruiter == null) return Forbid();

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.RecruiterId == recruiter.Id);
            if (job == null) return NotFound(new ApiErrorResponse("Job not found or access denied."));

            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.JobSeekerId == candidateId);
            if (resume == null || string.IsNullOrEmpty(resume.FilePath)) return NotFound(new ApiErrorResponse("Resume not found."));

            var absolutePath = Path.Combine(_fileSettings.BasePath, resume.FilePath);
            if (!System.IO.File.Exists(absolutePath)) return NotFound(new ApiErrorResponse("Resume file not found on server."));

            var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
            return File(stream, resume.ContentType ?? "application/pdf", resume.FileName);
        }

        private static string FormatDateRange(DateTime startDate, DateTime? endDate, bool isCurrent, string lang = "en")
        {
            var culture = new System.Globalization.CultureInfo(lang == "ar" ? "ar-EG" : "en-US");
            var present = lang == "ar" ? "الحالي" : "Present";
            var start = startDate.ToString("MMM yyyy", culture);
            var end = isCurrent ? present : endDate?.ToString("MMM yyyy", culture) ?? present;
            return $"{start} - {end}";
        }

        private static string FormatDateRange(DateTime? startDate, DateTime? endDate, bool isCurrent, string lang = "en")
        {
            var culture = new System.Globalization.CultureInfo(lang == "ar" ? "ar-EG" : "en-US");
            var present = lang == "ar" ? "الحالي" : "Present";
            if (startDate == null && endDate == null && !isCurrent) return "Unknown Date";
            var start = startDate?.ToString("MMM yyyy", culture) ?? "Unknown";
            var end = isCurrent ? present : endDate?.ToString("MMM yyyy", culture) ?? "Unknown";
            return $"{start} - {end}";
        }

        /// <summary>
        /// Resolves the Accept-Language header to a two-letter language code ("en" or "ar").
        /// </summary>
        private static string GetLanguage(HttpContext httpContext)
        {
            var lang = httpContext.Request.Headers["Accept-Language"].FirstOrDefault();
            return string.IsNullOrEmpty(lang) || lang.StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        }

        /// <summary>
        /// Returns the localized name from a reference entity that has NameEn/NameAr columns.
        /// Falls back to NameEn if NameAr is null or the language is English.
        /// </summary>
        private static string? Localize(string? nameEn, string? nameAr, string lang)
        {
            return lang == "ar" ? (nameAr ?? nameEn) : nameEn;
        }

        /// <summary>
        /// Returns the localized name from a JobTitle entity that has TitleEn/TitleAr columns.
        /// </summary>
        private static string? LocalizeTitle(string? titleEn, string? titleAr, string lang)
        {
            return lang == "ar" ? (titleAr ?? titleEn) : titleEn;
        }

        /// <summary>
        /// Shared helper: maps a JobSeeker + optional cached Recommendation into a MatchedCandidateDto.
        /// Centralises JSON deserialisation and DTO assembly that was previously duplicated in three places:
        /// the AI-success path, the fallback path, and GetShortlistedCandidates.
        /// </summary>
        private static MatchedCandidateDto MapToMatchedCandidateDto(
            Models.JobSeeker.JobSeeker js,
            Models.Jobs.Recommendation? rec,
            List<string>? skills,
            bool isShortlisted,
            string defaultPicUrl,
            string lang = "en")
        {
            var matchedSkills = !string.IsNullOrEmpty(rec?.MatchedSkillsJson)
                ? JsonSerializer.Deserialize<List<string>>(rec!.MatchedSkillsJson) ?? new List<string>()
                : new List<string>();

            var missingSkills = !string.IsNullOrEmpty(rec?.MissingSkillsJson)
                ? JsonSerializer.Deserialize<List<string>>(rec!.MissingSkillsJson) ?? new List<string>()
                : new List<string>();

            return new MatchedCandidateDto
            {
                JobSeekerId     = js.Id,
                FullName        = $"{js.User.FirstName} {js.User.LastName}",
                ProfilePictureUrl = js.User.ProfilePictureUrl ?? defaultPicUrl,
                JobTitle        = LocalizeTitle(js.JobTitle?.TitleEn, js.JobTitle?.TitleAr, lang),
                Bio             = js.Bio,
                YearsOfExperience = js.YearsOfExperience,
                CountryName     = Localize(js.Country?.NameEn, js.Country?.NameAr, lang),
                CityName        = Localize(js.City?.NameEn, js.City?.NameAr, lang),
                AssessmentScore = js.CurrentAssessmentScore,
                IsAssessed      = js.CurrentAssessmentScore.HasValue,
                Skills          = skills ?? new List<string>(),
                MatchScore      = rec?.MatchScore ?? 0,
                MatchedSkills   = matchedSkills,
                MissingSkills   = missingSkills,
                AiReasoning     = rec?.AiReasoning,
                IsShortlisted   = isShortlisted
            };
        }
    }
}
