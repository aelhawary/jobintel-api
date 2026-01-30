using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.JobSeeker
{
    /// <summary>
    /// Represents a job seeker's uploaded CV/Resume file
    /// </summary>
    public class Resume
    {
        public int Id { get; set; }
        
        [Required]
        public int JobSeekerId { get; set; }
        
        /// <summary>
        /// Original filename as uploaded by the user
        /// </summary>
        [Required, MaxLength(150)]
        public string FileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Stored filename on disk (with timestamp to avoid collisions)
        /// </summary>
        [Required, MaxLength(200)]
        public string StoredFileName { get; set; } = string.Empty;
        
        /// <summary>
        /// Relative path to the file from the storage root
        /// </summary>
        [Required, MaxLength(300)]
        public string FilePath { get; set; } = string.Empty;
        
        /// <summary>
        /// MIME type of the file (e.g., application/pdf)
        /// </summary>
        [Required, MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        /// <summary>
        /// File size in bytes
        /// </summary>
        [Required]
        public long FileSizeBytes { get; set; }
        
        /// <summary>
        /// Status of CV parsing (for future AI parsing feature)
        /// Values: Pending, Processing, Completed, Failed
        /// </summary>
        [MaxLength(20)]
        public string ParseStatus { get; set; } = "Pending";
        
        /// <summary>
        /// Timestamp when the CV was parsed (null if not yet parsed)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Soft delete flag - when true, the resume is considered deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;
        
        /// <summary>
        /// Timestamp when the resume was soft-deleted
        /// </summary>
        public DateTime? DeletedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public JobSeeker JobSeeker { get; set; } = null!;
    }
}
