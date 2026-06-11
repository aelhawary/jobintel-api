using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Request DTO for creating/updating education entry
    /// </summary>
    public class EducationRequestDto : IValidatableObject
    {
        /// <summary>
        /// School/University name
        /// </summary>
        /// <example>Stanford University</example>
        [Required(ErrorMessage = "Institution name is required")]
        [MaxLength(150, ErrorMessage = "Institution name cannot exceed 150 characters")]
        public string Institution { get; set; } = string.Empty;

        /// <summary>
        /// Degree level: HighSchool, Diploma, Associate, Bachelor, Master, PhD, Other
        /// </summary>
        /// <example>Bachelor</example>
        [Required(ErrorMessage = "Degree is required")]
        public Degree Degree { get; set; }

        /// <summary>
        /// Field of Study / Major (ID from reference table, null if custom)
        /// </summary>
        /// <example>Computer Science</example>
        public int? FieldOfStudyId { get; set; }

        /// <summary>
        /// Custom field of study text (used when FieldOfStudyId is 0)
        /// </summary>
        /// <example>Computer and Communication Engineering</example>
        [MaxLength(150)]
        public string? FieldOfStudyName { get; set; }

        /// <summary>
        /// Start date (YYYY-MM format accepted)
        /// </summary>
        /// <example>2017-09-01</example>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date or expected graduation (null if currently studying)
        /// </summary>
        /// <example>2021-06-01</example>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether currently studying here
        /// </summary>
        /// <example>false</example>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// GPA or grade (e.g., "3.8/4.0", "First Class Honours")
        /// </summary>
        /// <example>3.8/4.0</example>
        [MaxLength(50, ErrorMessage = "Grade/GPA cannot exceed 50 characters")]
        public string? GradeOrGPA { get; set; }

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
                    "End date is required when this is not your current study",
                    new[] { nameof(EndDate) });
            }

            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
            {
                yield return new ValidationResult(
                    "End date must be on or after start date",
                    new[] { nameof(EndDate), nameof(StartDate) });
            }
        }
    }

    /// <summary>
    /// Response DTO for education entry
    /// </summary>
    public class EducationResponseDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// School/University name
        /// </summary>
        public string Institution { get; set; } = string.Empty;

        /// <summary>
        /// Degree level
        /// </summary>
        public Degree Degree { get; set; }

        /// <summary>
        /// Field of Study / Major
        /// </summary>
        public string FieldOfStudy { get; set; } = string.Empty;

        /// <summary>
        /// Field of Study ID
        /// </summary>
        public int? FieldOfStudyId { get; set; }

        /// <summary>
        /// Raw field of study text (used when no DB match is found)
        /// </summary>
        public string? FieldOfStudyName { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date (null if currently studying)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether currently studying here
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// GPA or grade
        /// </summary>
        public string? GradeOrGPA { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Formatted date range (e.g., "Sep 2017 - Jun 2021" or "Sep 2023 - Present")
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
    /// Response for education list operations
    /// </summary>
    public class EducationListResponseDto
    {
        /// <summary>
        /// List of education entries
        /// </summary>
        public List<EducationResponseDto> EducationList { get; set; } = new();

        /// <summary>
        /// Total number of education entries
        /// </summary>
        public int TotalCount { get; set; }
    }
}
