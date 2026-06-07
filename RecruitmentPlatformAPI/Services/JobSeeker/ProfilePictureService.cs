using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Enums;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    /// <summary>
    /// Service for managing user profile pictures
    /// </summary>
    public class ProfilePictureService : IProfilePictureService
    {
        private readonly AppDbContext _context;
        private readonly FileStorageSettings _fileSettings;
        private readonly ILogger<ProfilePictureService> _logger;
        private readonly string _storagePath;

        // Image magic bytes for validation
        private static readonly Dictionary<string, byte[][]> ImageMagicBytes = new()
        {
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".webp", new[] { 
                new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF header (need to check WEBP after)
            }}
        };

        public ProfilePictureService(
            AppDbContext context,
            IOptions<FileStorageSettings> fileSettings,
            ILogger<ProfilePictureService> logger)
        {
            _context = context;
            _fileSettings = fileSettings.Value;
            _logger = logger;
            _storagePath = _fileSettings.GetProfilePicturesStoragePath();
            
            // Ensure storage directory exists
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        public async Task<ProfilePictureUploadResultDto> UploadProfilePictureAsync(
            int userId, Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Validate the file
                var validation = await ValidateImageFileAsync(fileStream, fileName, contentType, fileStream.Length);
                if (!validation.IsValid)
                {
                    return new ProfilePictureUploadResultDto
                    {
                        Success = false,
                        Message = validation.ErrorMessage!
                    };
                }

                // Get user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ProfilePictureUploadResultDto
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Delete existing uploaded picture if any
                await DeleteExistingProfilePictureFileAsync(userId);

                // Generate unique filename
                var storedFileName = GenerateStoredFileName(userId, validation.Extension!);
                var filePath = Path.Combine(_storagePath, storedFileName);

                // Reset stream position
                fileStream.Position = 0;

                // Save file to disk
                using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStreamWriter);
                }

                // Use role-specific route so stored URL matches the actual controller endpoint.
                var pictureUrl = BuildProfilePictureUrl(user.AccountType);
                user.ProfilePictureUrl = pictureUrl;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Profile picture uploaded for user {UserId}: {FileName}", userId, storedFileName);

                return new ProfilePictureUploadResultDto
                {
                    Success = true,
                    Message = "Profile picture uploaded successfully",
                    Url = pictureUrl,
                    FileSizeBytes = fileStream.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture for user {UserId}", userId);
                return new ProfilePictureUploadResultDto
                {
                    Success = false,
                    Message = "An error occurred while uploading the profile picture"
                };
            }
        }

        public async Task<ProfilePictureResponseDto> GetProfilePictureAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ProfilePictureResponseDto
                {
                    HasProfilePicture = false
                };
            }

            // Check if user has any profile picture URL
            if (string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                return new ProfilePictureResponseDto
                {
                    HasProfilePicture = false
                };
            }

            // Check if it's an OAuth picture (external URL) or uploaded picture (local URL)
            var isOAuthPicture = IsOAuthPictureUrl(user.ProfilePictureUrl);

            // For uploaded pictures, get file info
            if (!isOAuthPicture)
            {
                var fileInfo = GetUploadedPictureFileInfo(userId);
                if (fileInfo != null)
                {
                    return new ProfilePictureResponseDto
                    {
                        HasProfilePicture = true,
                        IsOAuthPicture = false,
                        Url = user.ProfilePictureUrl,
                        OriginalFileName = fileInfo.Name,
                        ContentType = GetContentTypeFromExtension(fileInfo.Extension),
                        FileSizeBytes = fileInfo.Length,
                        UploadedAt = fileInfo.CreationTimeUtc
                    };
                }
            }

            // OAuth picture
            return new ProfilePictureResponseDto
            {
                HasProfilePicture = true,
                IsOAuthPicture = true,
                Url = user.ProfilePictureUrl
            };
        }

        public async Task<(Stream? stream, string? contentType, string? fileName)?> GetProfilePictureFileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                return null;
            }

            // If it's an OAuth picture, we can't serve it directly
            if (IsOAuthPictureUrl(user.ProfilePictureUrl))
            {
                return null;
            }

            // Find the uploaded file
            var fileInfo = GetUploadedPictureFileInfo(userId);
            if (fileInfo == null || !fileInfo.Exists)
            {
                return null;
            }

            var contentType = GetContentTypeFromExtension(fileInfo.Extension);
            var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);

            return (stream, contentType, fileInfo.Name);
        }

        public async Task<bool> DeleteProfilePictureAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Delete file if exists
                var deleted = await DeleteExistingProfilePictureFileAsync(userId);

                // Clear profile picture URL (even for OAuth pictures, user can choose to remove)
                user.ProfilePictureUrl = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Profile picture deleted for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile picture for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> HasUploadedProfilePictureAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                return false;
            }

            // Check if it's not an OAuth picture and file exists
            if (!IsOAuthPictureUrl(user.ProfilePictureUrl))
            {
                var fileInfo = GetUploadedPictureFileInfo(userId);
                return fileInfo != null && fileInfo.Exists;
            }

            return false;
        }

        public async Task<ImageValidationResult> ValidateImageFileAsync(
            Stream fileStream, string fileName, string contentType, long fileSize)
        {
            // Check file size
            if (fileSize > _fileSettings.MaxProfilePictureSizeBytes)
            {
                var maxSizeMB = _fileSettings.MaxProfilePictureSizeBytes / (1024 * 1024);
                return ImageValidationResult.Invalid($"File size exceeds maximum allowed size of {maxSizeMB}MB");
            }

            if (fileSize == 0)
            {
                return ImageValidationResult.Invalid("File is empty");
            }

            // Get and validate extension
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                return ImageValidationResult.Invalid("File must have an extension");
            }

            if (!_fileSettings.IsImageExtensionAllowed(extension))
            {
                return ImageValidationResult.Invalid(
                    $"File type not allowed. Allowed types: {string.Join(", ", _fileSettings.AllowedImageExtensions)}");
            }

            // Validate MIME type
            if (!_fileSettings.IsImageMimeTypeAllowed(contentType))
            {
                return ImageValidationResult.Invalid(
                    $"Content type not allowed. Allowed types: {string.Join(", ", _fileSettings.AllowedImageMimeTypes)}");
            }

            // Validate magic bytes (file signature)
            var magicBytesValid = await ValidateImageMagicBytesAsync(fileStream, extension);
            if (!magicBytesValid)
            {
                return ImageValidationResult.Invalid("File content does not match the declared file type");
            }

            return ImageValidationResult.Valid(extension, contentType);
        }

        #region Private Helper Methods

        private async Task<bool> ValidateImageMagicBytesAsync(Stream fileStream, string extension)
        {
            if (!ImageMagicBytes.TryGetValue(extension, out var expectedHeaders))
            {
                return false;
            }

            var maxHeaderLength = expectedHeaders.Max(h => h.Length);
            var headerBuffer = new byte[Math.Max(maxHeaderLength, 12)]; // 12 for WEBP check

            fileStream.Position = 0;
            var bytesRead = await fileStream.ReadAsync(headerBuffer, 0, headerBuffer.Length);

            if (bytesRead < expectedHeaders.Min(h => h.Length))
            {
                return false;
            }

            // Special handling for WEBP (RIFF....WEBP format)
            if (extension == ".webp")
            {
                // Check RIFF header and WEBP identifier at offset 8
                var isRiff = headerBuffer[0] == 0x52 && headerBuffer[1] == 0x49 && 
                             headerBuffer[2] == 0x46 && headerBuffer[3] == 0x46;
                var isWebp = bytesRead >= 12 && 
                             headerBuffer[8] == 0x57 && headerBuffer[9] == 0x45 && 
                             headerBuffer[10] == 0x42 && headerBuffer[11] == 0x50;
                return isRiff && isWebp;
            }

            // Standard magic bytes check
            foreach (var expectedHeader in expectedHeaders)
            {
                if (bytesRead >= expectedHeader.Length)
                {
                    var match = true;
                    for (int i = 0; i < expectedHeader.Length; i++)
                    {
                        if (headerBuffer[i] != expectedHeader[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match) return true;
                }
            }

            return false;
        }

        private string GenerateStoredFileName(int userId, string extension)
        {
            // Use userId in filename for easy lookup
            return $"profile_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        }

        private FileInfo? GetUploadedPictureFileInfo(int userId)
        {
            if (!Directory.Exists(_storagePath))
            {
                return null;
            }

            // Look for files matching the user's profile picture pattern
            var pattern = $"profile_{userId}_*.*";
            var files = Directory.GetFiles(_storagePath, pattern);

            if (files.Length == 0)
            {
                return null;
            }

            // Return the most recent file
            return files
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTimeUtc)
                .FirstOrDefault();
        }

        private Task<bool> DeleteExistingProfilePictureFileAsync(int userId)
        {
            var pattern = $"profile_{userId}_*.*";
            var files = Directory.Exists(_storagePath) 
                ? Directory.GetFiles(_storagePath, pattern) 
                : Array.Empty<string>();

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    _logger.LogInformation("Deleted existing profile picture file: {File}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete existing profile picture file: {File}", file);
                }
            }

            return Task.FromResult(files.Length > 0);
        }

        private bool IsOAuthPictureUrl(string url)
        {
            // OAuth pictures are external URLs (Google, etc.)
            return url.StartsWith("http://") || url.StartsWith("https://") 
                ? !url.Contains(_fileSettings.BaseUrl)
                : false;
        }

        private string BuildProfilePictureUrl(AccountType accountType)
        {
            var routePrefix = accountType == AccountType.Recruiter
                ? "recruiter"
                : "jobseeker";

            return $"{_fileSettings.BaseUrl}/api/{routePrefix}/picture";
        }

        private string GetContentTypeFromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }
}
