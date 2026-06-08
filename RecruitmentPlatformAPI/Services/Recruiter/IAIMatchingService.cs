using RecruitmentPlatformAPI.DTOs.Recruiter;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    /// <summary>
    /// Service for integrating with the external AI matching engine.
    /// Handles pre-filtering candidates, calling the AI API, and mapping results.
    /// Results are cached per job to avoid spamming the external API.
    /// </summary>
    public interface IAIMatchingService
    {
        /// <summary>
        /// Fetches pre-filtered candidates for a job and sends them to the AI matching engine.
        /// Returns cached results if available; otherwise calls the external API and caches the response.
        /// </summary>
        Task<AIMatchingResponse?> GetMatchesAsync(int jobId, int maxResults = 10);

        /// <summary>
        /// Invalidates the cached AI matching results for a job.
        /// Call this when job requirements change so the next request re-fetches from the AI API.
        /// </summary>
        Task InvalidateCacheAsync(int jobId);
    }
}
