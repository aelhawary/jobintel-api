using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Response DTO for resume/CV information
    /// </summary>
    public class ResumeDto
    {
        /// <summary>
        /// Unique identifier of the resume
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Original filename as uploaded by the user
        /// </summary>
        /// <example>John_Doe_CV.pdf</example>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// MIME type of the file
        /// </summary>
        /// <example>application/pdf</example>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// File size in bytes
        /// </summary>
        /// <example>1048576</example>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Human-readable file size
        /// </summary>
        /// <example>1.00 MB</example>
        public string FileSizeDisplay { get; set; } = string.Empty;

        /// <summary>
        /// URL to download the resume
        /// </summary>
        /// <example>/api/resume/download</example>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Status of CV parsing (Pending, Processing, Completed, Failed)
        /// </summary>
        /// <example>Pending</example>
        public string ParseStatus { get; set; } = string.Empty;

        /// <summary>
        /// When the resume was uploaded
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the resume was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response wrapper for resume operations
    /// </summary>
    public class ResumeResponseDto
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        /// <example>Resume uploaded successfully</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The resume data (null if operation failed or no resume exists)
        /// </summary>
        public ResumeDto? Resume { get; set; }

        /// <summary>
        /// The parsed structured data from the CV
        /// </summary>
        public ParsedResumeDataDto? ExtractedData { get; set; }

        /// <summary>
        /// Current step of the profile wizard
        /// </summary>
        /// <example>1</example>
        public int CurrentStep { get; set; } = 1;

        public static ResumeResponseDto SuccessResult(ResumeDto resume, string message = "Operation successful")
        {
            return new ResumeResponseDto
            {
                Success = true,
                Message = message,
                Resume = resume,
                CurrentStep = 1
            };
        }

        public static ResumeResponseDto FailureResult(string message)
        {
            return new ResumeResponseDto
            {
                Success = false,
                Message = message,
                Resume = null,
                CurrentStep = 1
            };
        }
    }

    /// <summary>
    /// Validation result for file upload
    /// </summary>
    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static FileValidationResult Valid() => new() { IsValid = true };
        public static FileValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
    }

    public class ParsedResumeDataDto
    {
        public int? JobTitleId { get; set; }
        public string? JobTitleName { get; set; }
        public int? YearsOfExperience { get; set; }
        public int? CountryId { get; set; }
        public string? CountryName { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? FirstLanguageId { get; set; }
        public string? FirstLanguageName { get; set; }
        public string? Bio { get; set; }

        public List<ParsedExperienceDto> Experiences { get; set; } = new();
        public List<ParsedEducationDto> Educations { get; set; } = new();
        public List<ParsedProjectDto> Projects { get; set; } = new();
        public List<int> SkillIds { get; set; } = new();
        public ParsedSocialAccountDto? SocialAccounts { get; set; }
    }

    public class ParsedExperienceDto
    {
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? CountryName { get; set; }
        public int? CountryId { get; set; }
        public string? CityName { get; set; }
        public int? CityId { get; set; }
        public RecruitmentPlatformAPI.Enums.EmploymentType EmploymentType { get; set; } = RecruitmentPlatformAPI.Enums.EmploymentType.FullTime;
        public string? Responsibilities { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ParsedEducationDto
    {
        public string? Institution { get; set; }
        public string? Degree { get; set; }
        public int? FieldOfStudyId { get; set; }
        public string? FieldOfStudyName { get; set; }
        public string? GradeOrGpa { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ParsedProjectDto
    {
        public string? Title { get; set; }
        public string? TechnologiesUsed { get; set; }
        public string? Description { get; set; }
        public string? ProjectLink { get; set; }
    }

    public class ParsedSocialAccountDto
    {
        public string? LinkedIn { get; set; }
        public string? Github { get; set; }
        public string? Behance { get; set; }
        public string? Dribbble { get; set; }
        public string? PersonalWebsite { get; set; }
    }
}
