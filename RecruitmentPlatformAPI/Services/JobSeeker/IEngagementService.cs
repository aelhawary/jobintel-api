using RecruitmentPlatformAPI.DTOs.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    /// <summary>
    /// Service for tracking and querying job seeker engagement analytics
    /// (profile views, search appearances).
    /// </summary>
    public interface IEngagementService
    {
        /// <summary>
        /// Record that a job seeker's profile appeared in recruiter search results.
        /// </summary>
        Task RecordSearchAppearancesAsync(IEnumerable<int> jobSeekerIds, int? recruiterId);

        /// <summary>
        /// Record that a recruiter clicked into a specific job seeker's full profile.
        /// </summary>
        Task RecordProfileViewAsync(int jobSeekerId, int recruiterId);

        /// <summary>
        /// Get engagement statistics for a specific job seeker (for their dashboard widget).
        /// </summary>
        Task<EngagementStatsDto> GetEngagementStatsAsync(int jobSeekerId);
    }
}
