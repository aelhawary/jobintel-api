using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    /// <summary>
    /// Tracks when a recruiter views or encounters a job seeker's profile
    /// (via search results or direct profile click).
    /// Used for engagement analytics on the Job Seeker dashboard.
    /// </summary>
    public class ProfileView
    {
        public int Id { get; set; }

        /// <summary>
        /// The job seeker whose profile was viewed/appeared.
        /// </summary>
        [Required]
        public int JobSeekerId { get; set; }

        /// <summary>
        /// The recruiter who triggered the view (nullable for anonymous/system views).
        /// </summary>
        public int? ViewerRecruiterId { get; set; }

        /// <summary>
        /// The job that was being browsed when this view occurred.
        /// Provides context for search appearances (which job listing triggered this).
        /// </summary>
        public int? JobId { get; set; }

        /// <summary>
        /// Type of view: "Search" (appeared in search results) or "ProfileClick" (recruiter clicked into full profile).
        /// </summary>
        [Required, MaxLength(20)]
        public string ViewType { get; set; } = "Search";

        /// <summary>
        /// When the view occurred.
        /// </summary>
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
        public RecruitmentPlatformAPI.Models.Recruiter.Recruiter? ViewerRecruiter { get; set; }
    }
}
