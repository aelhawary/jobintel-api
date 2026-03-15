using RecruitmentPlatformAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Request DTO for creating/updating work experience
    /// </summary>
    public class ExperienceRequestDto
    {
        /// <summary>
        /// Job title/position
        /// </summary>
        /// <example>Senior Product Designer</example>
        [Required(ErrorMessage = "Job title is required")]
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// Company/organization name
        /// </summary>
        /// <example>TechFlow Inc.</example>
        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Location/City of the job
        /// </summary>
        /// <example>San Francisco, CA</example>
        [MaxLength(150, ErrorMessage = "Location cannot exceed 150 characters")]
        public string? Location { get; set; }

        /// <summary>
        /// Type of employment
        /// <summary>
        /// Start date of employment (YYYY-MM format accepted)
        /// </summary>
        /// <example>2021-01-01</example>
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of employment (null if current position)
        /// </summary>
        /// <example>null</example>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether this is the current position
        /// </summary>
        /// <example>true</example>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Display order (lower numbers appear first)
        /// </summary>
        /// <example>0</example>
        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// Response DTO for work experience
    /// </summary>
    public class ExperienceResponseDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Job title/position
        /// </summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// Company/organization name
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Location/City of the job
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Start date of employment
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of employment (null if current)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether this is the current position
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Formatted date range (e.g., "Jan 2021 - Present")
        /// </summary>
        public string DateRange { get; set; } = string.Empty;

        /// <summary>
        /// When this record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response for experience list operations
    /// </summary>
    public class ExperienceListResponseDto
    {
        /// <summary>
        /// List of experience entries
        /// </summary>
        public List<ExperienceResponseDto> Experiences { get; set; } = new();

        /// <summary>
        /// Total number of experiences
        /// </summary>
        public int TotalCount { get; set; }
    }
}
