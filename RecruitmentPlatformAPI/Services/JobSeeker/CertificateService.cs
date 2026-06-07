using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
{
    public class CertificateService : ICertificateService
    {
        private readonly AppDbContext _context;
        private readonly FileStorageSettings _fileSettings;
        private readonly ILogger<CertificateService> _logger;
        private readonly IWebHostEnvironment _environment;

        public CertificateService(
            AppDbContext context,
            IOptions<FileStorageSettings> fileSettings,
            ILogger<CertificateService> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _fileSettings = fileSettings.Value;
            _logger = logger;
            _environment = environment;
        }

        public async Task<CertificateListResponseDto> GetCertificatesAsync(int userId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null)
                {
                    return new CertificateListResponseDto
                    {
                        Success = false,
                        Message = "Only job seekers can access certificates"
                    };
                }

                var certificates = await _context.Certificates
                    .Where(c => c.JobSeekerId == jobSeeker.Id && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                return new CertificateListResponseDto
                {
                    Success = true,
                    Message = certificates.Count > 0
                        ? $"Found {certificates.Count} certificate(s)"
                        : "No certificates found",
                    Certificates = certificates.Select(MapToDto).ToList(),
                    TotalCount = certificates.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificates for user {UserId}", userId);
                return new CertificateListResponseDto { Success = false, Message = "An error occurred" };
            }
        }

        public async Task<CertificateResponseDto?> GetCertificateByIdAsync(int userId, int certificateId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null)
                    return CertificateResponseDto.FailureResult("Only job seekers can access certificates");

                var certificate = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.Id == certificateId && c.JobSeekerId == jobSeeker.Id && !c.IsDeleted);

                if (certificate == null)
                    return CertificateResponseDto.FailureResult("Certificate not found");

                return CertificateResponseDto.SuccessResult(MapToDto(certificate), "Certificate retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting certificate {CertificateId} for user {UserId}", certificateId, userId);
                return CertificateResponseDto.FailureResult("An error occurred");
            }
        }

        public async Task<CertificateResponseDto?> AddCertificateAsync(int userId, CertificateRequestDto dto, IFormFile? file)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null)
                    return CertificateResponseDto.FailureResult("Only job seekers can add certificates");

                // Validate dates
                if (dto.IssueDate.HasValue && dto.ExpirationDate.HasValue && dto.ExpirationDate < dto.IssueDate)
                    return CertificateResponseDto.FailureResult("Expiration date must be after issue date");

                // Get next display order
                var maxOrder = await _context.Certificates
                    .Where(c => c.JobSeekerId == jobSeeker.Id && !c.IsDeleted)
                    .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;

                var certificate = new Certificate
                {
                    JobSeekerId = jobSeeker.Id,
                    Title = dto.Title.Trim(),
                    IssuingOrganization = dto.IssuingOrganization?.Trim(),
                    IssueDate = dto.IssueDate,
                    ExpirationDate = dto.ExpirationDate,
                    DisplayOrder = maxOrder + 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Handle file upload
                if (file != null && file.Length > 0)
                {
                    var fileError = ValidateCertificateFile(file);
                    if (fileError != null)
                        return CertificateResponseDto.FailureResult(fileError);

                    await SaveCertificateFile(certificate, file, userId);
                }

                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added certificate {CertificateId} for user {UserId}", certificate.Id, userId);
                return CertificateResponseDto.SuccessResult(MapToDto(certificate), "Certificate added successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding certificate for user {UserId}", userId);
                return CertificateResponseDto.FailureResult("An error occurred while adding the certificate");
            }
        }

        public async Task<CertificateResponseDto?> UpdateCertificateAsync(int userId, int certificateId, CertificateRequestDto dto, IFormFile? file)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null)
                    return CertificateResponseDto.FailureResult("Only job seekers can update certificates");

                var certificate = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.Id == certificateId && c.JobSeekerId == jobSeeker.Id && !c.IsDeleted);

                if (certificate == null)
                    return CertificateResponseDto.FailureResult("Certificate not found");

                // Validate dates
                if (dto.IssueDate.HasValue && dto.ExpirationDate.HasValue && dto.ExpirationDate < dto.IssueDate)
                    return CertificateResponseDto.FailureResult("Expiration date must be after issue date");

                certificate.Title = dto.Title.Trim();
                certificate.IssuingOrganization = dto.IssuingOrganization?.Trim();
                certificate.IssueDate = dto.IssueDate;
                certificate.ExpirationDate = dto.ExpirationDate;
                certificate.UpdatedAt = DateTime.UtcNow;

                // Handle file upload (replace existing if provided)
                if (file != null && file.Length > 0)
                {
                    var fileError = ValidateCertificateFile(file);
                    if (fileError != null)
                        return CertificateResponseDto.FailureResult(fileError);

                    // Delete old file if exists
                    DeletePhysicalFile(certificate);
                    await SaveCertificateFile(certificate, file, userId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated certificate {CertificateId} for user {UserId}", certificateId, userId);
                return CertificateResponseDto.SuccessResult(MapToDto(certificate), "Certificate updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating certificate {CertificateId} for user {UserId}", certificateId, userId);
                return CertificateResponseDto.FailureResult("An error occurred while updating the certificate");
            }
        }

        public async Task<bool> DeleteCertificateAsync(int userId, int certificateId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return false;

                var certificate = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.Id == certificateId && c.JobSeekerId == jobSeeker.Id && !c.IsDeleted);

                if (certificate == null) return false;

                // Soft delete
                certificate.IsDeleted = true;
                certificate.DeletedAt = DateTime.UtcNow;
                certificate.UpdatedAt = DateTime.UtcNow;

                // Delete physical file
                DeletePhysicalFile(certificate);

                // Reorder remaining certificates
                var remaining = await _context.Certificates
                    .Where(c => c.JobSeekerId == jobSeeker.Id && !c.IsDeleted)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                for (int i = 0; i < remaining.Count; i++)
                {
                    remaining[i].DisplayOrder = i + 1;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted certificate {CertificateId} for user {UserId}", certificateId, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting certificate {CertificateId} for user {UserId}", certificateId, userId);
                return false;
            }
        }

        public async Task<(Stream? FileStream, string ContentType, string FileName)?> DownloadCertificateFileAsync(int userId, int certificateId)
        {
            try
            {
                var jobSeeker = await GetJobSeekerAsync(userId);
                if (jobSeeker == null) return null;

                var certificate = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.Id == certificateId && c.JobSeekerId == jobSeeker.Id && !c.IsDeleted);

                if (certificate == null || string.IsNullOrEmpty(certificate.FilePath))
                    return null;

                var absolutePath = Path.Combine(GetAbsoluteBasePath(), certificate.FilePath);
                if (!File.Exists(absolutePath))
                {
                    _logger.LogError("Certificate file not found on disk: {Path}", absolutePath);
                    return null;
                }

                var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return (fileStream, certificate.ContentType ?? "application/octet-stream", certificate.FileName ?? "certificate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading certificate file {CertificateId} for user {UserId}", certificateId, userId);
                return null;
            }
        }

        #region Private Helpers

        private async Task<Models.JobSeeker.JobSeeker?> GetJobSeekerAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.AccountType != Enums.AccountType.JobSeeker)
                return null;

            return await _context.JobSeekers.FirstOrDefaultAsync(j => j.UserId == userId);
        }

        private string? ValidateCertificateFile(IFormFile file)
        {
            if (file.Length > _fileSettings.MaxFileSizeBytes)
            {
                var maxSizeMB = _fileSettings.MaxFileSizeBytes / (1024.0 * 1024.0);
                return $"File size exceeds the maximum allowed size of {maxSizeMB:F0} MB.";
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_fileSettings.IsExtensionAllowed(extension))
            {
                var allowedExtensions = string.Join(", ", _fileSettings.AllowedExtensions);
                return $"Invalid file type. Allowed types: {allowedExtensions}";
            }

            return null;
        }

        private async Task SaveCertificateFile(Certificate certificate, IFormFile file, int userId)
        {
            var storagePath = Path.Combine(GetAbsoluteBasePath(), _fileSettings.CertificatesFolder);
            if (!Directory.Exists(storagePath))
                Directory.CreateDirectory(storagePath);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var storedFileName = $"{userId}_cert_{timestamp}{extension}";
            var relativePath = Path.Combine(_fileSettings.CertificatesFolder, storedFileName);
            var absolutePath = Path.Combine(GetAbsoluteBasePath(), relativePath);

            await using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            certificate.FileName = file.FileName;
            certificate.StoredFileName = storedFileName;
            certificate.FilePath = relativePath;
            certificate.ContentType = file.ContentType;
            certificate.FileSizeBytes = file.Length;
        }

        private void DeletePhysicalFile(Certificate certificate)
        {
            if (string.IsNullOrEmpty(certificate.FilePath)) return;

            var absolutePath = Path.Combine(GetAbsoluteBasePath(), certificate.FilePath);
            if (File.Exists(absolutePath))
            {
                try { File.Delete(absolutePath); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete certificate file: {Path}", absolutePath); }
            }
        }

        private string GetAbsoluteBasePath()
        {
            if (Path.IsPathRooted(_fileSettings.BasePath))
                return _fileSettings.BasePath;
            return Path.Combine(_environment.ContentRootPath, _fileSettings.BasePath);
        }

        private CertificateDto MapToDto(Certificate cert)
        {
            return new CertificateDto
            {
                Id = cert.Id,
                Title = cert.Title,
                IssuingOrganization = cert.IssuingOrganization,
                IssueDate = cert.IssueDate,
                ExpirationDate = cert.ExpirationDate,
                HasFile = !string.IsNullOrEmpty(cert.FilePath),
                FileName = cert.FileName,
                FileSizeBytes = cert.FileSizeBytes,
                FileSizeDisplay = cert.FileSizeBytes.HasValue ? FormatFileSize(cert.FileSizeBytes.Value) : null,
                DownloadUrl = !string.IsNullOrEmpty(cert.FilePath) ? $"/api/jobseeker/certificates/{cert.Id}/download" : null,
                DisplayOrder = cert.DisplayOrder,
                CreatedAt = cert.CreatedAt,
                UpdatedAt = cert.UpdatedAt
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1) { order++; size /= 1024; }
            return $"{size:0.##} {sizes[order]}";
        }

        #endregion
    }
}
