using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.Profile
{
    /// <summary>
    /// Service for managing resume/CV file operations
    /// </summary>
    public class ResumeService : IResumeService
    {
        private readonly AppDbContext _context;
        private readonly FileStorageSettings _fileSettings;
        private readonly ILogger<ResumeService> _logger;
        private readonly IWebHostEnvironment _environment;

        public ResumeService(
            AppDbContext context,
            IOptions<FileStorageSettings> fileSettings,
            ILogger<ResumeService> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _fileSettings = fileSettings.Value;
            _logger = logger;
            _environment = environment;
        }

        /// <inheritdoc />
        public async Task<ResumeResponseDto> UploadResumeAsync(int userId, IFormFile file)
        {
            try
            {
                // Validate file
                var validation = ValidateFile(file);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("File validation failed for user {UserId}: {Error}", userId, validation.ErrorMessage);
                    return ResumeResponseDto.FailureResult(validation.ErrorMessage);
                }

                // Get job seeker
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(j => j.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogWarning("JobSeeker not found for user {UserId}", userId);
                    return ResumeResponseDto.FailureResult("Job seeker profile not found. Please complete Step 1 first.");
                }

                // Check for existing resume
                var existingResume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id && !r.IsDeleted);

                // Ensure storage directory exists
                var storagePath = GetAbsoluteStoragePath();
                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                    _logger.LogInformation("Created storage directory: {Path}", storagePath);
                }

                // Generate unique filename
                var originalFileName = file.FileName;
                var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var storedFileName = $"{userId}_{timestamp}{extension}";
                var relativePath = Path.Combine(_fileSettings.ResumeFolder, storedFileName);
                var absolutePath = Path.Combine(GetAbsoluteBasePath(), relativePath);

                // If there's an existing resume, delete the old file
                if (existingResume != null)
                {
                    var oldFilePath = Path.Combine(GetAbsoluteBasePath(), existingResume.FilePath);
                    if (File.Exists(oldFilePath))
                    {
                        try
                        {
                            File.Delete(oldFilePath);
                            _logger.LogInformation("Deleted old resume file: {Path}", oldFilePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old resume file: {Path}", oldFilePath);
                        }
                    }
                }

                // Save the new file
                await using (var stream = new FileStream(absolutePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Saved resume file: {Path} ({Size} bytes)", absolutePath, file.Length);

                Resume resume;

                if (existingResume != null)
                {
                    // Update existing resume
                    existingResume.FileName = originalFileName;
                    existingResume.StoredFileName = storedFileName;
                    existingResume.FilePath = relativePath;
                    existingResume.ContentType = file.ContentType;
                    existingResume.FileSizeBytes = file.Length;
                    existingResume.ParseStatus = "Pending";
                    existingResume.ProcessedAt = null;
                    existingResume.UpdatedAt = DateTime.UtcNow;

                    resume = existingResume;
                    _logger.LogInformation("Updated existing resume {ResumeId} for user {UserId}", resume.Id, userId);
                }
                else
                {
                    // Create new resume
                    resume = new Resume
                    {
                        JobSeekerId = jobSeeker.Id,
                        FileName = originalFileName,
                        StoredFileName = storedFileName,
                        FilePath = relativePath,
                        ContentType = file.ContentType,
                        FileSizeBytes = file.Length,
                        ParseStatus = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Resumes.Add(resume);
                    _logger.LogInformation("Created new resume for user {UserId}", userId);
                }

                await _context.SaveChangesAsync();

                return ResumeResponseDto.SuccessResult(
                    MapToDto(resume),
                    existingResume != null ? "Resume replaced successfully" : "Resume uploaded successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading resume for user {UserId}", userId);
                return ResumeResponseDto.FailureResult("An error occurred while uploading the resume. Please try again.");
            }
        }

        /// <inheritdoc />
        public async Task<ResumeResponseDto> GetResumeAsync(int userId)
        {
            try
            {
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(j => j.UserId == userId);

                if (jobSeeker == null)
                {
                    return ResumeResponseDto.FailureResult("Job seeker profile not found.");
                }

                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id && !r.IsDeleted);

                if (resume == null)
                {
                    return new ResumeResponseDto
                    {
                        Success = true,
                        Message = "No resume uploaded yet.",
                        Resume = null,
                        CurrentStep = 3
                    };
                }

                return ResumeResponseDto.SuccessResult(MapToDto(resume), "Resume retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resume for user {UserId}", userId);
                return ResumeResponseDto.FailureResult("An error occurred while retrieving the resume.");
            }
        }

        /// <inheritdoc />
        public async Task<(Stream? FileStream, string ContentType, string FileName)?> GetResumeFileAsync(int userId)
        {
            try
            {
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(j => j.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogWarning("JobSeeker not found for user {UserId}", userId);
                    return null;
                }

                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id && !r.IsDeleted);

                if (resume == null)
                {
                    _logger.LogWarning("No resume found for user {UserId}", userId);
                    return null;
                }

                var absolutePath = Path.Combine(GetAbsoluteBasePath(), resume.FilePath);

                if (!File.Exists(absolutePath))
                {
                    _logger.LogError("Resume file not found on disk: {Path}", absolutePath);
                    return null;
                }

                var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return (fileStream, resume.ContentType, resume.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resume file for user {UserId}", userId);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ResumeResponseDto> DeleteResumeAsync(int userId)
        {
            try
            {
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(j => j.UserId == userId);

                if (jobSeeker == null)
                {
                    return ResumeResponseDto.FailureResult("Job seeker profile not found.");
                }

                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id && !r.IsDeleted);

                if (resume == null)
                {
                    return ResumeResponseDto.FailureResult("No resume found to delete.");
                }

                // Soft delete
                resume.IsDeleted = true;
                resume.DeletedAt = DateTime.UtcNow;
                resume.UpdatedAt = DateTime.UtcNow;

                // Optionally delete the physical file
                var absolutePath = Path.Combine(GetAbsoluteBasePath(), resume.FilePath);
                if (File.Exists(absolutePath))
                {
                    try
                    {
                        File.Delete(absolutePath);
                        _logger.LogInformation("Deleted resume file: {Path}", absolutePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete resume file: {Path}", absolutePath);
                        // Continue with soft delete even if file deletion fails
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted resume {ResumeId} for user {UserId}", resume.Id, userId);

                return new ResumeResponseDto
                {
                    Success = true,
                    Message = "Resume deleted successfully",
                    Resume = null,
                    CurrentStep = 3
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting resume for user {UserId}", userId);
                return ResumeResponseDto.FailureResult("An error occurred while deleting the resume.");
            }
        }

        /// <inheritdoc />
        public FileValidationResult ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return FileValidationResult.Invalid("No file provided or file is empty.");
            }

            // Check file size
            if (file.Length > _fileSettings.MaxFileSizeBytes)
            {
                var maxSizeMB = _fileSettings.MaxFileSizeBytes / (1024.0 * 1024.0);
                return FileValidationResult.Invalid($"File size exceeds the maximum allowed size of {maxSizeMB:F0} MB.");
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_fileSettings.IsExtensionAllowed(extension))
            {
                var allowedExtensions = string.Join(", ", _fileSettings.AllowedExtensions);
                return FileValidationResult.Invalid($"Invalid file type. Allowed types: {allowedExtensions}");
            }

            // Check MIME type
            if (!_fileSettings.IsMimeTypeAllowed(file.ContentType))
            {
                var allowedExtensions = string.Join(", ", _fileSettings.AllowedExtensions);
                return FileValidationResult.Invalid($"Invalid file type. Allowed types: {allowedExtensions}");
            }

            // Validate file content based on type
            if (extension == ".pdf" && !IsValidPdfFile(file))
            {
                return FileValidationResult.Invalid("The file does not appear to be a valid PDF document.");
            }
            
            if (extension == ".docx" && !IsValidDocxFile(file))
            {
                return FileValidationResult.Invalid("The file does not appear to be a valid DOCX document.");
            }

            return FileValidationResult.Valid();
        }

        /// <inheritdoc />
        public async Task<bool> HasResumeAsync(int userId)
        {
            var jobSeeker = await _context.JobSeekers
                .FirstOrDefaultAsync(j => j.UserId == userId);

            if (jobSeeker == null)
            {
                return false;
            }

            return await _context.Resumes
                .AnyAsync(r => r.JobSeekerId == jobSeeker.Id && !r.IsDeleted);
        }

        #region Private Helper Methods

        private string GetAbsoluteBasePath()
        {
            // If BasePath is absolute, use it directly; otherwise, combine with content root
            if (Path.IsPathRooted(_fileSettings.BasePath))
            {
                return _fileSettings.BasePath;
            }

            return Path.Combine(_environment.ContentRootPath, _fileSettings.BasePath);
        }

        private string GetAbsoluteStoragePath()
        {
            return Path.Combine(GetAbsoluteBasePath(), _fileSettings.ResumeFolder);
        }

        private ResumeDto MapToDto(Resume resume)
        {
            return new ResumeDto
            {
                Id = resume.Id,
                FileName = resume.FileName,
                ContentType = resume.ContentType,
                FileSizeBytes = resume.FileSizeBytes,
                FileSizeDisplay = FormatFileSize(resume.FileSizeBytes),
                DownloadUrl = "/api/resume/download",
                ParseStatus = resume.ParseStatus,
                CreatedAt = resume.CreatedAt,
                UpdatedAt = resume.UpdatedAt
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        private static bool IsValidPdfFile(IFormFile file)
        {
            try
            {
                // PDF files start with "%PDF-" (hex: 25 50 44 46 2D)
                using var stream = file.OpenReadStream();
                var header = new byte[5];
                var bytesRead = stream.Read(header, 0, 5);

                if (bytesRead < 5)
                {
                    return false;
                }

                // Check for PDF magic bytes
                return header[0] == 0x25 && // %
                       header[1] == 0x50 && // P
                       header[2] == 0x44 && // D
                       header[3] == 0x46 && // F
                       header[4] == 0x2D;   // -
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidDocxFile(IFormFile file)
        {
            try
            {
                // DOCX files are ZIP archives with specific magic bytes (PK..)
                // ZIP file header starts with 0x50 0x4B 0x03 0x04
                using var stream = file.OpenReadStream();
                var header = new byte[4];
                var bytesRead = stream.Read(header, 0, 4);

                if (bytesRead < 4)
                {
                    return false;
                }

                // Check for ZIP/DOCX magic bytes
                return header[0] == 0x50 && // P
                       header[1] == 0x4B && // K
                       header[2] == 0x03 && 
                       header[3] == 0x04;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
