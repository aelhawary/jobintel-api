using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    public class AIMatchingService : IAIMatchingService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AIMatchingService> _logger;

        private const string AI_API_URL = "https://alikhaled123-ai-recruitment-api.hf.space/api/recommend";
        private const string CACHE_KEY_PREFIX = "JobMatches_";
        // 5-minute TTL balances data freshness (new candidates appear quickly) with API cost savings
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

        public AIMatchingService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<AIMatchingService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<AIMatchingResponse?> GetMatchesAsync(int jobId, int maxResults = 10)
        {
            // ── Check cache first (key includes maxResults to prevent wrong-count cache reuse) ──
            var cacheKey = $"{CACHE_KEY_PREFIX}{jobId}_{maxResults}";
            if (_cache.TryGetValue(cacheKey, out CachedMatchResult? cached) && cached != null)
            {
                _logger.LogInformation("Cache HIT for Job {JobId} (maxResults={MaxResults}). Returning {Count} cached candidates.",
                    jobId, maxResults, cached.Response?.Results?.Count ?? 0);
                return cached.Response;
            }

            _logger.LogInformation("Cache MISS for Job {JobId}. Fetching fresh results from AI API.", jobId);

            try
            {
                // 1. Load the job with its skills and title (to derive role family)
                var job = await _context.Jobs
                    .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                    .Include(j => j.JobTitle)
                    .Include(j => j.Country)
                    .Include(j => j.City)
                    .FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);

                if (job == null)
                {
                    _logger.LogWarning("Job {JobId} not found or inactive", jobId);
                    return null;
                }

                var requiredSkillNames = job.JobSkills.Select(js => js.Skill.Name).ToList();

                // 2. Resolve all job title IDs that share the same RoleFamily as the job,
                //    plus any closely related families.
                //
                //    Rationale: FullStack developers are universally considered for Backend AND
                //    Frontend roles. Similarly, a FullStack job opening can accept pure Backend
                //    or Frontend candidates. This map encodes those industry-standard overlaps.
                List<int>? roleFamilyTitleIds = null;
                if (job.JobTitleId.HasValue && job.JobTitle != null)
                {
                    var targetFamily = job.JobTitle.RoleFamily;

                    // Define which families are cross-eligible for each family.
                    // A job in family X will also consider candidates from any family in this list.
                    var relatedFamilies = new Dictionary<JobTitleRoleFamily, List<JobTitleRoleFamily>>
                    {
                        [JobTitleRoleFamily.Backend]   = new() { JobTitleRoleFamily.FullStack },
                        [JobTitleRoleFamily.Frontend]  = new() { JobTitleRoleFamily.FullStack },
                        [JobTitleRoleFamily.FullStack] = new() { JobTitleRoleFamily.Backend, JobTitleRoleFamily.Frontend },
                        [JobTitleRoleFamily.DevOps]    = new() { JobTitleRoleFamily.Backend },
                        [JobTitleRoleFamily.Data]      = new() { JobTitleRoleFamily.Backend },
                    };

                    var familiesToInclude = new HashSet<JobTitleRoleFamily> { targetFamily };
                    if (relatedFamilies.TryGetValue(targetFamily, out var extras))
                        foreach (var f in extras) familiesToInclude.Add(f);

                    roleFamilyTitleIds = await _context.JobTitles
                        .Where(jt => familiesToInclude.Contains(jt.RoleFamily) && jt.IsActive)
                        .Select(jt => jt.Id)
                        .ToListAsync();

                    _logger.LogInformation(
                        "Job {JobId} (family={Family}): expanded title filter to {Count} related titles across {FamilyCount} families.",
                        jobId, targetFamily, roleFamilyTitleIds.Count, familiesToInclude.Count);
                }

                // 3. Pre-filter candidates:
                //    - Active account
                //    - Years of experience >= job minimum (±1 year tolerance)
                //    - Job title within the same role family (if the job specifies a title)
                //    - Assessment score is optional — unassessed candidates still participate
                var preFilteredCandidates = await _context.JobSeekers
                    .Include(js => js.User)
                    .Include(js => js.JobTitle)
                    .Include(js => js.Country)
                    .Include(js => js.City)
                    .Where(js =>
                        js.User.IsActive &&
                        js.YearsOfExperience != null &&
                        js.YearsOfExperience >= (job.MinYearsOfExperience - 1) &&
                        (roleFamilyTitleIds == null || (js.JobTitleId.HasValue && roleFamilyTitleIds.Contains(js.JobTitleId.Value))))
                    .ToListAsync();

                if (!preFilteredCandidates.Any())
                {
                    _logger.LogInformation("No pre-filtered candidates found for Job {JobId}", jobId);
                    var emptyResponse = new AIMatchingResponse
                    {
                        Job = MapJobToAI(job, requiredSkillNames),
                        MaxResults = maxResults,
                        Results = new List<AIResult>()
                    };

                    _cache.Set(cacheKey, new CachedMatchResult(emptyResponse), CACHE_DURATION);
                    return emptyResponse;
                }

                // 3. Load skills for all pre-filtered candidates in one query
                var candidateIds = preFilteredCandidates.Select(js => js.Id).ToList();
                var candidateSkills = await _context.JobSeekerSkills
                    .Where(jss => candidateIds.Contains(jss.JobSeekerId))
                    .Include(jss => jss.Skill)
                    .GroupBy(jss => jss.JobSeekerId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(jss => jss.Skill.Name).ToList());

                // 4. Build the AI request payload
                //    Batch-load experiences and educations for all candidates (avoids N+1).
                var allExperiences = await _context.Experiences
                    .Where(e => candidateIds.Contains(e.JobSeekerId) && !e.IsDeleted)
                    .OrderByDescending(e => e.StartDate)
                    .GroupBy(e => e.JobSeekerId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(e =>
                            $"{e.JobTitle} at {e.CompanyName} ({e.StartDate:MMM yyyy} - {(e.EndDate.HasValue ? e.EndDate.Value.ToString("MMM yyyy") : "Present")})").ToList());

                var allEducations = await _context.Educations
                    .Where(e => candidateIds.Contains(e.JobSeekerId) && !e.IsDeleted)
                    .Include(e => e.FieldOfStudy)
                    .OrderByDescending(e => e.StartDate)
                    .GroupBy(e => e.JobSeekerId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(e =>
                            $"{e.Degree} in {e.FieldOfStudy?.NameEn ?? e.FieldOfStudyName ?? "N/A"} from {e.Institution}").ToList());

                var aiRequest = new AIMatchingRequest
                {
                    Job = MapJobToAI(job, requiredSkillNames),
                    MaxResults = maxResults,
                    Candidates = preFilteredCandidates.Select(js => new AICandidateInfo
                    {
                        CandidateId = js.Id.ToString(),
                        FullName = $"{js.User.FirstName} {js.User.LastName}",
                        TotalYearsExp = js.YearsOfExperience ?? 0,
                        Bio = js.Bio,
                        ExperienceDetails = allExperiences.ContainsKey(js.Id)
                            ? string.Join("; ", allExperiences[js.Id])
                            : string.Empty,
                        Skills = string.Join(", ", candidateSkills.GetValueOrDefault(js.Id, new List<string>())),
                        Education = allEducations.ContainsKey(js.Id)
                            ? string.Join("; ", allEducations[js.Id])
                            : string.Empty,
                        TestScoreSoftTech = js.CurrentAssessmentScore
                    }).ToList()
                };

                // 5. Call the external AI matching API
                var httpClient = _httpClientFactory.CreateClient();
                var jsonPayload = JsonSerializer.Serialize(aiRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation(
                    "Calling AI Matching API for Job {JobId} with {Count} candidates",
                    jobId, preFilteredCandidates.Count);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(AI_API_URL, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "AI Matching API returned {StatusCode} for Job {JobId}: {Error}",
                        response.StatusCode, jobId, errorBody);
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var aiResponse = JsonSerializer.Deserialize<AIMatchingResponse>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (aiResponse == null)
                {
                    _logger.LogWarning("AI Matching API returned null response for Job {JobId}", jobId);
                    return null;
                }

                _logger.LogInformation(
                    "AI Matching API returned {Count} results for Job {JobId}",
                    aiResponse.Results?.Count ?? 0, jobId);

                // Use shorter TTL (5 min) for empty results to allow retries
                var ttl = (aiResponse.Results?.Count ?? 0) == 0
                    ? TimeSpan.FromMinutes(5)
                    : CACHE_DURATION;

                _cache.Set(cacheKey, new CachedMatchResult(aiResponse), ttl);

                return aiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI Matching API for Job {JobId}", jobId);
                return null;
            }
        }

        public Task InvalidateCacheAsync(int jobId)
        {
            // The cache key now includes maxResults (e.g. "JobMatches_5_10", "JobMatches_5_20").
            // IMemoryCache has no wildcard removal, so we remove the most common maxResults values.
            // This covers all practical cases without requiring a distributed cache.
            var commonMaxResults = new[] { 5, 10, 15, 20, 25, 50 };
            foreach (var n in commonMaxResults)
            {
                _cache.Remove($"{CACHE_KEY_PREFIX}{jobId}_{n}");
            }
            _logger.LogInformation("Cache invalidated for Job {JobId} (all maxResults variants).", jobId);
            return Task.CompletedTask;
        }

        private static AIJobInfo MapJobToAI(Models.Jobs.Job job, List<string> requiredSkillNames)
        {
            return new AIJobInfo
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                MinYearsOfExperience = job.MinYearsOfExperience,
                RequiredSkills = requiredSkillNames
            };
        }

        /// <summary>
        /// Wrapper for cached responses. Prevents stale reference issues with IMemoryCache.
        /// </summary>
        private record CachedMatchResult(AIMatchingResponse? Response);
    }
}
