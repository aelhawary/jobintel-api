using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Recruiter;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    public class AIMatchingService : IAIMatchingService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AIMatchingService> _logger;

        private const string AI_API_URL = "https://alikhaled123-ai-recruitment-api.hf.space";
        private const string CACHE_KEY_PREFIX = "JobMatches_";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

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
            // ── Check cache first ──
            var cacheKey = $"{CACHE_KEY_PREFIX}{jobId}";
            if (_cache.TryGetValue(cacheKey, out CachedMatchResult? cached) && cached != null)
            {
                _logger.LogInformation("Cache HIT for Job {JobId}. Returning cached AI match results ({Count} candidates).",
                    jobId, cached.Response?.Results?.Count ?? 0);
                return cached.Response;
            }

            _logger.LogInformation("Cache MISS for Job {JobId}. Fetching fresh results from AI API.", jobId);

            try
            {
                // 1. Load the job with its skills
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

                // 2. Pre-filter candidates from our database
                //    Criteria: has assessment score, years of experience >= min, user is active
                var preFilteredCandidates = await _context.JobSeekers
                    .Include(js => js.User)
                    .Include(js => js.JobTitle)
                    .Include(js => js.Country)
                    .Include(js => js.City)
                    .Where(js =>
                        js.User.IsActive &&
                        js.CurrentAssessmentScore != null &&
                        js.YearsOfExperience != null &&
                        js.YearsOfExperience >= job.MinYearsOfExperience &&
                        (!job.JobTitleId.HasValue || js.JobTitleId == job.JobTitleId))
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
                            $"{e.Degree} in {e.FieldOfStudy?.NameEn ?? "N/A"} from {e.Institution}").ToList());

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
            var cacheKey = $"{CACHE_KEY_PREFIX}{jobId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Cache invalidated for Job {JobId}. Next request will re-fetch from AI API.", jobId);
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
