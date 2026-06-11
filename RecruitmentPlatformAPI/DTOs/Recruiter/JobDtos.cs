using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.DTOs.Recruiter
{
    // ═══════════════════════════════════════════════════════════
    //  REQUEST DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Request DTO for creating or updating a job posting
    /// </summary>
    public class JobRequestDto : IValidatableObject
    {
        /// <summary>
        /// Job title
        /// </summary>
        /// <example>Senior Backend Developer</example>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Job title ID (FK to JobTitle reference table)
        /// </summary>
        /// <example>12</example>
        [Required(ErrorMessage = "Job title is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Job title must be a valid ID")]
        public int JobTitleId { get; set; }

        /// <summary>
        /// Detailed job description
        /// </summary>
        /// <example>We are looking for an experienced backend developer to join our team...</example>
        [Required(ErrorMessage = "Description is required")]
        [StringLength(1200, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 1200 characters")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Job requirements and qualifications
        /// </summary>
        /// <example>5+ years experience with C# and ASP.NET Core, strong knowledge of SQL Server...</example>
        [Required(ErrorMessage = "Requirements are required")]
        [StringLength(1200, MinimumLength = 20, ErrorMessage = "Requirements must be between 20 and 1200 characters")]
        public string Requirements { get; set; } = string.Empty;

        /// <summary>
        /// Employment type: FullTime, PartTime, Freelance, or Internship
        /// </summary>
        /// <example>FullTime</example>
        [Required(ErrorMessage = "Employment type is required")]
        [EnumDataType(typeof(EmploymentType), ErrorMessage = "Employment type must be a valid value")]
        public EmploymentType EmploymentType { get; set; }

        /// <summary>
        /// Minimum years of experience required (0-30)
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = "Minimum years of experience is required")]
        [Range(0, 30, ErrorMessage = "Minimum years of experience must be between 0 and 30")]
        public int MinYearsOfExperience { get; set; }

        /// <summary>
        /// Work Model: Remote, Hybrid, or OnSite
        /// </summary>
        /// <example>Remote</example>
        [Required(ErrorMessage = "Work model is required")]
        [EnumDataType(typeof(WorkModel), ErrorMessage = "Work model must be a valid value")]
        public WorkModel WorkModel { get; set; }

        /// <summary>
        /// Country ID
        /// </summary>
        public int? CountryId { get; set; }

        /// <summary>
        /// City ID
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// List of skill IDs to associate with this job (optional, max 25)
        /// </summary>
        /// <example>[1, 5, 12]</example>
        [MaxLength(25, ErrorMessage = "Maximum 25 skills allowed per job")]
        public List<int>? SkillIds { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (WorkModel == WorkModel.OnSite || WorkModel == WorkModel.Hybrid)
            {
                if (!CountryId.HasValue || CountryId.Value <= 0)
                {
                    yield return new ValidationResult("Country is required for On-Site or Hybrid jobs", new[] { nameof(CountryId) });
                }
                
                if (!CityId.HasValue || CityId.Value <= 0)
                {
                    yield return new ValidationResult("City is required for On-Site or Hybrid jobs", new[] { nameof(CityId) });
                }
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  RESPONSE DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Response DTO for a single job posting
    /// </summary>
    public class JobResponseDto
    {
        /// <summary>Job ID</summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>Job title</summary>
        /// <example>Senior Backend Developer</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>Job title ID (FK to JobTitle reference table)</summary>
        /// <example>12</example>
        public int? JobTitleId { get; set; }

        /// <summary>Job title name (from reference table)</summary>
        /// <example>Backend Developer</example>
        public string? JobTitleName { get; set; }

        /// <summary>Job description</summary>
        /// <example>We are looking for an experienced backend developer...</example>
        public string Description { get; set; } = string.Empty;

        /// <summary>Job requirements</summary>
        /// <example>5+ years experience with C# and ASP.NET Core...</example>
        public string Requirements { get; set; } = string.Empty;

        /// <summary>Employment type</summary>
        /// <example>FullTime</example>
        public EmploymentType EmploymentType { get; set; }

        /// <summary>Minimum years of experience</summary>
        /// <example>3</example>
        public int MinYearsOfExperience { get; set; }

        /// <summary>Work Model</summary>
        /// <example>Remote</example>
        public WorkModel WorkModel { get; set; }

        /// <summary>Country ID</summary>
        public int? CountryId { get; set; }

        /// <summary>Country Name</summary>
        public string? CountryName { get; set; }

        /// <summary>City ID</summary>
        public int? CityId { get; set; }

        /// <summary>City Name</summary>
        public string? CityName { get; set; }

        /// <summary>Date the job was posted</summary>
        public DateTime PostedAt { get; set; }

        /// <summary>Date the job was last updated</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Whether the job is currently active</summary>
        /// <example>true</example>
        public bool IsActive { get; set; }

        /// <summary>Number of candidates who applied/matched (reserved for AI module)</summary>
        /// <example>0</example>
        public int CandidateCount { get; set; }

        /// <summary>Skills associated with this job</summary>
        public List<JobSkillDto> Skills { get; set; } = new();
    }

    /// <summary>
    /// Skill associated with a job posting
    /// </summary>
    public class JobSkillDto
    {
        /// <summary>Skill ID</summary>
        /// <example>5</example>
        public int Id { get; set; }

        /// <summary>Skill name</summary>
        /// <example>C#</example>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Paginated list of job postings
    /// </summary>
    public class JobListResponseDto
    {
        /// <summary>List of jobs on this page</summary>
        public List<JobResponseDto> Jobs { get; set; } = new();

        /// <summary>Total number of matching jobs</summary>
        /// <example>25</example>
        public int TotalCount { get; set; }

        /// <summary>Current page number</summary>
        /// <example>1</example>
        public int Page { get; set; }

        /// <summary>Number of items per page</summary>
        /// <example>10</example>
        public int PageSize { get; set; }

        /// <summary>Total number of pages</summary>
        /// <example>3</example>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Skill option for dropdown/autocomplete in job creation form
    /// </summary>
    public class SkillOptionDto
    {
        /// <summary>Skill ID</summary>
        /// <example>5</example>
        public int Id { get; set; }

        /// <summary>Skill name</summary>
        /// <example>C#</example>
        public string Name { get; set; } = string.Empty;
    }
}
