using System.ComponentModel.DataAnnotations;
using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Request DTO for creating/updating education entry
    /// </summary>
    public class EducationRequestDto
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
        /// Field of Study / Major
        /// </summary>
        /// <example>Computer Science</example>
        [Required(ErrorMessage = "Field of study is required")]
        [MaxLength(150, ErrorMessage = "Field of study cannot exceed 150 characters")]
        public string FieldOfStudy { get; set; } = string.Empty;

        /// <summary>
        /// Grade or GPA (optional)
        /// <summary>
        /// Start date (YYYY-MM format accepted)
        /// </summary>
        /// <example>2017-09-01</example>
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

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
        /// Display order (lower numbers appear first)
        /// </summary>
        /// <example>0</example>
        public int DisplayOrder { get; set; } = 0;
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
        /// Start date
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date (null if currently studying)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether currently studying here
        /// </summary>
        public bool IsCurrent { get; set; }

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
