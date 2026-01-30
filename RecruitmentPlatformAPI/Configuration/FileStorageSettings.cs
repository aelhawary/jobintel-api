namespace RecruitmentPlatformAPI.Configuration
{
    /// <summary>
    /// Configuration settings for file storage (CV uploads, etc.)
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
        /// Maximum file size in bytes (default: 5MB)
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5 MB

        /// <summary>
        /// Allowed file extensions for resume uploads
        /// </summary>
        public string[] AllowedExtensions { get; set; } = new[] { ".pdf" };

        /// <summary>
        /// Allowed MIME types for resume uploads
        /// </summary>
        public string[] AllowedMimeTypes { get; set; } = new[] { "application/pdf" };

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
        /// Validates if the file extension is allowed
        /// </summary>
        public bool IsExtensionAllowed(string extension)
        {
            return AllowedExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Validates if the MIME type is allowed
        /// </summary>
        public bool IsMimeTypeAllowed(string mimeType)
        {
            return AllowedMimeTypes.Contains(mimeType.ToLowerInvariant());
        }
    }
}
