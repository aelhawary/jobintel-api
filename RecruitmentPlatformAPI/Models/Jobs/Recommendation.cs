using System.ComponentModel.DataAnnotations;
using JobSeekerModel = RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker;

namespace RecruitmentPlatformAPI.Models.Jobs
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

        /// <summary>
        /// AI-generated reasoning explaining why this candidate is a good/bad fit.
        /// </summary>
        [MaxLength(2000)]
        public string? AiReasoning { get; set; }

        /// <summary>
        /// JSON-serialized list of skills that matched between candidate and job requirements.
        /// Example: "[\"Python\",\"Machine Learning\",\"SQL\"]"
        /// </summary>
        [MaxLength(2000)]
        public string? MatchedSkillsJson { get; set; }

        /// <summary>
        /// JSON-serialized list of required skills the candidate is missing.
        /// Example: "[\"TensorFlow\",\"Spark\"]"
        /// </summary>
        [MaxLength(2000)]
        public string? MissingSkillsJson { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Job Job { get; set; } = null!;
        public JobSeekerModel JobSeeker { get; set; } = null!;
    }
}
