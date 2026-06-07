using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    public class Certificate
    {
        public int Id { get; set; }
        [Required]
        public int JobSeekerId { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(150)]
        public string? IssuingOrganization { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // File storage fields (optional — certificate can be added without a file)
        [MaxLength(150)]
        public string? FileName { get; set; }
        [MaxLength(200)]
        public string? StoredFileName { get; set; }
        [MaxLength(300)]
        public string? FilePath { get; set; }
        [MaxLength(100)]
        public string? ContentType { get; set; }
        public long? FileSizeBytes { get; set; }

        [Required]
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
    }
}
