using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.DTOs.Recruiter;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    /// <summary>
    /// Service for tracking and querying job seeker engagement analytics
    /// (profile views, search appearances, AI recommendations).
    /// </summary>
    public interface IEngagementService
    {
        /// <summary>
        /// Record that job seekers' profiles appeared in recruiter search results.
        /// Includes 1-hour deduplication per recruiter+jobseeker pair.
        /// </summary>
        Task RecordSearchAppearancesAsync(IEnumerable<int> jobSeekerIds, int? recruiterId, int? jobId);

        /// <summary>
        /// Record that a recruiter clicked into a specific job seeker's full profile.
        /// Includes 1-hour deduplication.
        /// </summary>
        Task RecordProfileViewAsync(int jobSeekerId, int recruiterId, int? jobId);

        /// <summary>
        /// Store AI matching results as Recommendation records.
        /// Replaces existing recommendations for the same job (idempotent).
        /// </summary>
        Task StoreRecommendationsAsync(int jobId, List<MatchedCandidateDto> candidates);

        /// <summary>
        /// Get consolidated engagement statistics for a specific job seeker (for their dashboard widget).
        /// Includes profile views, search appearances, and recommendation appearances.
        /// Uses optimized SQL aggregation (no in-memory filtering).
        /// </summary>
        Task<EngagementStatsDto> GetEngagementStatsAsync(int jobSeekerId);
    }
}
