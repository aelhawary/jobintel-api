using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class Resume
    {
        public int Id { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [Required, MaxLength(150)]
        public string FileName { get; set; } = string.Empty;
        [Required, MaxLength(300)]
        public string FileUrl { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string FileType { get; set; } = string.Empty;
        [Required]
        public long FileSizeBytes { get; set; }
        [MaxLength(20)]
        public string ParseStatus { get; set; } = "Pending";
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
    }
}
