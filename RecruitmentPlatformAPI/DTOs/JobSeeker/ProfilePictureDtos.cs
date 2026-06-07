using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Response DTO for profile picture operations
    /// </summary>
    public class ProfilePictureResponseDto
    {
        /// <summary>
        /// URL to access the profile picture
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Whether the user has a profile picture
        /// </summary>
        public bool HasProfilePicture { get; set; }

        /// <summary>
        /// Indicates if the picture is from an OAuth provider (e.g., Google)
        /// </summary>
        public bool IsOAuthPicture { get; set; }

        /// <summary>
        /// Original filename (only for uploaded pictures, not OAuth)
        /// </summary>
        public string? OriginalFileName { get; set; }

        /// <summary>
        /// MIME type of the picture
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// File size in bytes (only for uploaded pictures)
        /// </summary>
        public long? FileSizeBytes { get; set; }

        /// <summary>
        /// When the picture was uploaded
        /// </summary>
        public DateTime? UploadedAt { get; set; }
    }

    /// <summary>
    /// Result of profile picture upload operation
    /// </summary>
    public class ProfilePictureUploadResultDto
    {
        /// <summary>
        /// Whether the upload was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// URL to access the uploaded profile picture
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long? FileSizeBytes { get; set; }
    }

    /// <summary>
    /// Result of image file validation
    /// </summary>
    public class ImageValidationResult
    {
        /// <summary>
        /// Whether the file passed all validation checks
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The sanitized file extension
        /// </summary>
        public string? Extension { get; set; }

        /// <summary>
        /// The detected content type
        /// </summary>
        public string? ContentType { get; set; }

        public static ImageValidationResult Valid(string extension, string contentType) => new()
        {
            IsValid = true,
            Extension = extension,
            ContentType = contentType
        };

        public static ImageValidationResult Invalid(string errorMessage) => new()
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
