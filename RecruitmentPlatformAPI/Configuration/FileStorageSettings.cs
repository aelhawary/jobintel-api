namespace RecruitmentPlatformAPI.Configuration
{
    /// <summary>
    /// Configuration settings for file storage (CV uploads, profile pictures, etc.)
    /// </summary>
    public class FileStorageSettings
    {
        /// <summary>
        /// Base path for file storage (relative to application root or absolute path)
        /// </summary>
        public string BasePath { get; set; } = "Uploads";

        /// <summary>
        /// Subdirectory for resume/CV files
        /// </summary>
        public string ResumeFolder { get; set; } = "Resumes";

        /// <summary>
        /// Subdirectory for profile picture files
        /// </summary>
        public string ProfilePicturesFolder { get; set; } = "ProfilePictures";

        /// <summary>
        /// Maximum file size in bytes for resumes (default: 10MB)
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Maximum file size in bytes for profile pictures (default: 2MB)
        /// </summary>
        public long MaxProfilePictureSizeBytes { get; set; } = 2 * 1024 * 1024; // 2 MB

        /// <summary>
        /// Allowed file extensions for resume uploads (PDF, DOCX)
        /// </summary>
        public string[] AllowedExtensions { get; set; } = new[] { ".pdf", ".docx" };

        /// <summary>
        /// Allowed file extensions for profile pictures
        /// </summary>
        public string[] AllowedImageExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        /// <summary>
        /// Allowed MIME types for resume uploads (PDF, DOCX)
        /// </summary>
        public string[] AllowedMimeTypes { get; set; } = new[] { 
            "application/pdf", 
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" 
        };

        /// <summary>
        /// Allowed MIME types for profile pictures
        /// </summary>
        public string[] AllowedImageMimeTypes { get; set; } = new[] { 
            "image/jpeg", 
            "image/png", 
            "image/webp" 
        };

        /// <summary>
        /// Base URL for serving files (used to construct download URLs)
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:5217";

        /// <summary>
        /// Gets the full path for resume storage
        /// </summary>
        public string GetResumeStoragePath()
        {
            return Path.Combine(BasePath, ResumeFolder);
        }

        /// <summary>
        /// Gets the full path for profile picture storage
        /// </summary>
        public string GetProfilePicturesStoragePath()
        {
            return Path.Combine(BasePath, ProfilePicturesFolder);
        }

        /// <summary>
        /// Validates if the file extension is allowed for resumes
        /// </summary>
        public bool IsExtensionAllowed(string extension)
        {
            return AllowedExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Validates if the file extension is allowed for images
        /// </summary>
        public bool IsImageExtensionAllowed(string extension)
        {
            return AllowedImageExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Validates if the MIME type is allowed for resumes
        /// </summary>
        public bool IsMimeTypeAllowed(string mimeType)
        {
            return AllowedMimeTypes.Contains(mimeType.ToLowerInvariant());
        }

        /// <summary>
        /// Validates if the MIME type is allowed for images
        /// </summary>
        public bool IsImageMimeTypeAllowed(string mimeType)
        {
            return AllowedImageMimeTypes.Contains(mimeType.ToLowerInvariant());
        }
    }
}
