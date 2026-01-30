using System.ComponentModel.DataAnnotations;
using JobSeekerModel = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Models.Reference
{
    public class Recommendation
    {
        public int Id { get; set; }
        [Required]
        public int JobId { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [Required]
        public decimal MatchScore { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Job Job { get; set; } = null!;
        public JobSeekerModel JobSeeker { get; set; } = null!;
    }
}
