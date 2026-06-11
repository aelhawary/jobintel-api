using RecruitmentPlatformAPI.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Request DTO for creating/updating work experience
    /// </summary>
    public class ExperienceRequestDto : IValidatableObject
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
        /// Country ID from reference table
        /// </summary>
        /// <example>65</example>
        public int? CountryId { get; set; }

        /// <summary>
        /// City ID from reference table
        /// </summary>
        /// <example>31769</example>
        public int? CityId { get; set; }

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
        /// Employment type (FullTime, PartTime, Contract, etc.)
        /// </summary>
        /// <example>FullTime</example>
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;

        /// <summary>
        /// Key responsibilities and achievements (max 2000 characters)
        /// </summary>
        /// <example>Designed and implemented REST APIs, Led a team of 5 developers</example>
        [MaxLength(2000, ErrorMessage = "Responsibilities cannot exceed 2000 characters")]
        public string? Responsibilities { get; set; }

        /// <summary>
        /// Display order (lower numbers appear first)
        /// </summary>
        /// <example>0</example>
        public int DisplayOrder { get; set; } = 0;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsCurrent && !EndDate.HasValue)
            {
                yield return new ValidationResult(
                    "End date is required when this is not your current position",
                    new[] { nameof(EndDate) });
            }

            if (EndDate.HasValue && EndDate.Value < StartDate)
            {
                yield return new ValidationResult(
                    "End date must be on or after start date",
                    new[] { nameof(EndDate), nameof(StartDate) });
            }
        }
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
        /// Country ID
        /// </summary>
        public int? CountryId { get; set; }

        /// <summary>
        /// Name of the Country
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// City ID
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// City name
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Employment type
        /// </summary>
        public EmploymentType EmploymentType { get; set; }

        /// <summary>
        /// Key responsibilities and achievements
        /// </summary>
        public string? Responsibilities { get; set; }

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
