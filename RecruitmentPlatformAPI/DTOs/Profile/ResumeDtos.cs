using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Profile
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
        /// Current step of the profile wizard
        /// </summary>
        /// <example>3</example>
        public int CurrentStep { get; set; } = 3;

        public static ResumeResponseDto SuccessResult(ResumeDto resume, string message = "Operation successful")
        {
            return new ResumeResponseDto
            {
                Success = true,
                Message = message,
                Resume = resume,
                CurrentStep = 3
            };
        }

        public static ResumeResponseDto FailureResult(string message)
        {
            return new ResumeResponseDto
            {
                Success = false,
                Message = message,
                Resume = null,
                CurrentStep = 3
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
}
