using System;
using System.ComponentModel.DataAnnotations;
using RecruitmentPlatformAPI.Models.Jobs;
using JobSeekerModel = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Models.Recruiter
{
    public class ShortlistedCandidate
    {
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public int JobSeekerId { get; set; }

        [Required]
        public int RecruiterId { get; set; }

        public DateTime ShortlistedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Job Job { get; set; } = null!;
        public JobSeekerModel JobSeeker { get; set; } = null!;
        public Recruiter Recruiter { get; set; } = null!;
    }
}
