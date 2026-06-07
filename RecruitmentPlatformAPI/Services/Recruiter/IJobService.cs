using RecruitmentPlatformAPI.DTOs.Recruiter;

namespace RecruitmentPlatformAPI.Services.Recruiter
{
    /// <summary>
    /// Service interface for managing recruiter job postings
    /// </summary>
    public interface IJobService
    {
        /// <summary>
        /// Create a new job posting with optional skills
        /// </summary>
        Task<JobResponseDto?> CreateJobAsync(int userId, JobRequestDto dto);

        /// <summary>
        /// Update an existing job posting (own jobs only)
        /// </summary>
        Task<JobResponseDto?> UpdateJobAsync(int userId, int jobId, JobRequestDto dto);

        /// <summary>
        /// Deactivate a job — hides it from candidate matching
        /// </summary>
        Task<bool> DeactivateJobAsync(int userId, int jobId);

        /// <summary>
        /// Reactivate a previously deactivated job
        /// </summary>
        Task<bool> ReactivateJobAsync(int userId, int jobId);

        /// <summary>
        /// Permanently delete a job posting (hard delete)
        /// </summary>
        Task<bool> DeleteJobAsync(int userId, int jobId);

        /// <summary>
        /// Get paginated list of jobs owned by the authenticated recruiter.
        /// Returns null if the user is not a recruiter.
        /// </summary>
        Task<JobListResponseDto?> GetMyJobsAsync(int userId, int page = 1, int pageSize = 10, bool? isActive = null);

        /// <summary>
        /// Get a specific job posting (own jobs only)
        /// </summary>
        Task<JobResponseDto?> GetJobByIdAsync(int userId, int jobId);

        /// <summary>
        /// Get available skills for the job creation form (with optional search filter)
        /// </summary>
        Task<List<SkillOptionDto>> GetSkillsAsync(string? search = null);

        /// <summary>
        /// Get the full profile of a candidate recommended for one of the recruiter's jobs.
        /// Returns null if: the job doesn't belong to this recruiter,
        /// or no Recommendation exists linking this job seeker to this job.
        /// </summary>
        Task<CandidateProfileDto?> GetCandidateProfileAsync(int userId, int jobId, int jobSeekerId);
    }
}
