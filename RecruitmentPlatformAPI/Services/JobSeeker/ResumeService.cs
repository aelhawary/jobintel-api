using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecruitmentPlatformAPI.Configuration;
using RecruitmentPlatformAPI.Data;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Models.JobSeeker;

namespace RecruitmentPlatformAPI.Services.JobSeeker
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
        private readonly ICvParserService _cvParserService;

        public ResumeService(
            AppDbContext context,
            IOptions<FileStorageSettings> fileSettings,
            ILogger<ResumeService> logger,
            IWebHostEnvironment environment,
            ICvParserService cvParserService)
        {
            _context = context;
            _fileSettings = fileSettings.Value;
            _logger = logger;
            _environment = environment;
            _cvParserService = cvParserService;
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

                // Replace the existing file (if any). The file system is the source of truth
                // for the *active* document; the Resume record tracks metadata only.
                var existingResume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id);

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

                _logger.LogInformation("Saved resume file: {Path} ({Size} bytes) for user {UserId}",
                    absolutePath, file.Length, userId);

                // Pure file-save path: NO AI extraction, NO profile mutation.
                // The wizard and the explicit "Parse & Auto-Fill" action are the only
                // two triggers allowed to call the AI. The resume starts in "Pending"
                // so the UI can show a clear state.
                Resume resume;
                if (existingResume != null)
                {
                    existingResume.FileName = originalFileName;
                    existingResume.StoredFileName = storedFileName;
                    existingResume.FilePath = relativePath;
                    existingResume.ContentType = file.ContentType;
                    existingResume.FileSizeBytes = file.Length;
                    existingResume.ParseStatus = "Pending";
                    existingResume.UpdatedAt = DateTime.UtcNow;
                    resume = existingResume;
                    _logger.LogInformation("Replaced existing resume {ResumeId} for user {UserId}", resume.Id, userId);
                }
                else
                {
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
        public async Task<ResumeResponseDto> ParseResumeAsync(int userId)
        {
            try
            {
                var jobSeeker = await _context.JobSeekers
                    .FirstOrDefaultAsync(j => j.UserId == userId);

                if (jobSeeker == null)
                {
                    _logger.LogWarning("JobSeeker not found for user {UserId}", userId);
                    return ResumeResponseDto.FailureResult("Job seeker profile not found. Please complete Step 1 first.");
                }

                var resume = await _context.Resumes
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id);

                if (resume == null)
                {
                    return ResumeResponseDto.FailureResult("No resume found. Please upload a resume first.");
                }

                var absolutePath = Path.Combine(GetAbsoluteBasePath(), resume.FilePath);
                if (!File.Exists(absolutePath))
                {
                    resume.ParseStatus = "Failed";
                    resume.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return ResumeResponseDto.FailureResult("Resume file is missing on the server. Please re-upload your resume.");
                }

                resume.ParseStatus = "Processing";
                resume.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Re-read the file from disk and run the AI pipeline.
                var extension = Path.GetExtension(resume.FilePath);
                ParsedResumeDataDto? extractedData = null;
                bool aiServiceFailed = false;
                try
                {
                    var text = ExtractTextFromFile(absolutePath, extension);
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        // File was readable but contained no text — likely
                        // image-based, encrypted, or empty.
                        resume.ParseStatus = "Failed";
                        resume.ProcessedAt = DateTime.UtcNow;
                        resume.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return ResumeResponseDto.FailureResult(
                            "We couldn't extract text from your CV. The file may be image-based, encrypted, or empty. Please upload a text-based PDF or DOCX.");
                    }
                    extractedData = await _cvParserService.ParseResumeTextAsync(text);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse CV text for resume {ResumeId}", resume.Id);
                    aiServiceFailed = true;
                }

                if (extractedData == null)
                {
                    resume.ParseStatus = "Failed";
                    resume.ProcessedAt = DateTime.UtcNow;
                    resume.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return ResumeResponseDto.FailureResult(
                        aiServiceFailed
                            ? "The AI service is temporarily unavailable due to high demand. Please try again in a few minutes."
                            : "We couldn't extract data from your CV. The file may be image-based, encrypted, or empty.");
                }

                // Apply ALL extracted data to the database in one transaction.
                // Personal Info is updated in addition to the list sections so the
                // "Auto-Fill" experience is truly exhaustive.
                await ApplyExtractedDataAsync(jobSeeker, extractedData);

                resume.ParseStatus = "Completed";
                resume.ProcessedAt = DateTime.UtcNow;
                resume.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var response = ResumeResponseDto.SuccessResult(
                    MapToDto(resume),
                    "CV parsed successfully. Your profile has been auto-filled."
                );
                response.ExtractedData = extractedData;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing resume for user {UserId}", userId);
                return ResumeResponseDto.FailureResult("An error occurred while parsing the resume. Please try again.");
            }
        }

        /// <summary>
        /// Persists a <see cref="ParsedResumeDataDto"/> to the database, overwriting the
        /// relevant sections of the given <paramref name="jobSeeker"/>. Updates:
        /// Personal Info (job title, years, country, city, phone, bio, language), Skills,
        /// Experiences, Educations, Projects, and Social Accounts. Soft-delete is used
        /// for the list entities (Experiences / Educations / Projects) to preserve audit
        /// history; Skills and Social Accounts are hard-replaced because the candidate
        /// is explicitly asking for a full re-parse.
        /// </summary>
        private async Task ApplyExtractedDataAsync(RecruitmentPlatformAPI.Models.JobSeeker.JobSeeker jobSeeker, ParsedResumeDataDto extractedData)
        {
            // 1) Personal Info (top-level fields on JobSeeker)
            if (extractedData.JobTitleId.HasValue && extractedData.JobTitleId.Value > 0)
                jobSeeker.JobTitleId = extractedData.JobTitleId;
            if (extractedData.YearsOfExperience.HasValue)
                jobSeeker.YearsOfExperience = extractedData.YearsOfExperience.Value;
            if (extractedData.CountryId.HasValue && extractedData.CountryId.Value > 0)
                jobSeeker.CountryId = extractedData.CountryId;
            if (extractedData.CityId.HasValue && extractedData.CityId.Value > 0)
                jobSeeker.CityId = extractedData.CityId;
            if (!string.IsNullOrWhiteSpace(extractedData.PhoneNumber))
                jobSeeker.PhoneNumber = extractedData.PhoneNumber.Trim();
            if (extractedData.FirstLanguageId.HasValue && extractedData.FirstLanguageId.Value > 0)
            {
                jobSeeker.FirstLanguageId = extractedData.FirstLanguageId;
                if (jobSeeker.FirstLanguageProficiency == null)
                    jobSeeker.FirstLanguageProficiency = RecruitmentPlatformAPI.Enums.LanguageProficiency.Advanced;
            }
            if (!string.IsNullOrWhiteSpace(extractedData.Bio))
                jobSeeker.Bio = extractedData.Bio.Trim().Length > 500
                    ? extractedData.Bio.Trim()[..500]
                    : extractedData.Bio.Trim();
            jobSeeker.UpdatedAt = DateTime.UtcNow;

            // 2) List sections — soft-delete old, insert new (same strategy as before).
            var oldEducations = await _context.Educations
                .Where(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted)
                .ToListAsync();
            var oldExperiences = await _context.Experiences
                .Where(e => e.JobSeekerId == jobSeeker.Id && !e.IsDeleted)
                .ToListAsync();
            var oldProjects = await _context.Projects
                .Where(p => p.JobSeekerId == jobSeeker.Id && !p.IsDeleted)
                .ToListAsync();
            var oldSkills = await _context.JobSeekerSkills
                .Where(s => s.JobSeekerId == jobSeeker.Id)
                .ToListAsync();
            var oldSocialAccount = await _context.SocialAccounts
                .FirstOrDefaultAsync(s => s.JobSeekerId == jobSeeker.Id);

            foreach (var e in oldEducations) { e.IsDeleted = true; e.DeletedAt = DateTime.UtcNow; e.UpdatedAt = DateTime.UtcNow; }
            foreach (var e in oldExperiences) { e.IsDeleted = true; e.DeletedAt = DateTime.UtcNow; e.UpdatedAt = DateTime.UtcNow; }
            foreach (var p in oldProjects) { p.IsDeleted = true; p.DeletedAt = DateTime.UtcNow; p.UpdatedAt = DateTime.UtcNow; }
            if (oldSkills.Any()) _context.JobSeekerSkills.RemoveRange(oldSkills);
            if (oldSocialAccount != null) _context.SocialAccounts.Remove(oldSocialAccount);

            // 3) Experiences
            if (extractedData.Experiences.Any())
            {
                for (int i = 0; i < extractedData.Experiences.Count; i++)
                {
                    var aiExp = extractedData.Experiences[i];
                    var exp = new Experience
                    {
                        JobSeekerId = jobSeeker.Id,
                        JobTitle = !string.IsNullOrWhiteSpace(aiExp.JobTitle) ? aiExp.JobTitle : "Unknown Title",
                        CompanyName = !string.IsNullOrWhiteSpace(aiExp.CompanyName) ? aiExp.CompanyName : "Unknown Company",
                        CountryId = aiExp.CountryId > 0 ? aiExp.CountryId.Value : (jobSeeker.CountryId ?? 0),
                        CityId = aiExp.CityId > 0 ? aiExp.CityId.Value : (jobSeeker.CityId ?? 0),
                        EmploymentType = aiExp.EmploymentType,
                        Responsibilities = !string.IsNullOrWhiteSpace(aiExp.Responsibilities) ? aiExp.Responsibilities.Trim() : null,
                        StartDate = aiExp.StartDate ?? new DateTime(2000, 1, 1),
                        EndDate = aiExp.IsCurrent ? null : aiExp.EndDate,
                        IsCurrent = aiExp.IsCurrent,
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    if (exp.CountryId > 0 && exp.CityId > 0)
                    {
                        _context.Experiences.Add(exp);
                    }
                }
            }

            // 4) Educations
            if (extractedData.Educations.Any())
            {
                for (int i = 0; i < extractedData.Educations.Count; i++)
                {
                    var aiEdu = extractedData.Educations[i];
                    var degreeEnum = Enum.TryParse<RecruitmentPlatformAPI.Enums.Degree>(aiEdu.Degree, true, out var d)
                        ? d
                        : RecruitmentPlatformAPI.Enums.Degree.Other;
                    var edu = new Education
                    {
                        JobSeekerId = jobSeeker.Id,
                        Institution = !string.IsNullOrWhiteSpace(aiEdu.Institution) ? aiEdu.Institution : "Unknown Institution",
                        Degree = degreeEnum,
                        FieldOfStudyId = aiEdu.FieldOfStudyId,
                        FieldOfStudyName = !string.IsNullOrWhiteSpace(aiEdu.FieldOfStudyName) ? aiEdu.FieldOfStudyName.Trim() : null,
                        GradeOrGPA = !string.IsNullOrWhiteSpace(aiEdu.GradeOrGpa) ? aiEdu.GradeOrGpa.Trim() : null,
                        StartDate = aiEdu.StartDate,
                        EndDate = aiEdu.IsCurrent ? null : aiEdu.EndDate,
                        IsCurrent = aiEdu.IsCurrent,
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Educations.Add(edu);
                }
            }

            // 5) Projects
            if (extractedData.Projects.Any())
            {
                for (int i = 0; i < extractedData.Projects.Count; i++)
                {
                    var aiProj = extractedData.Projects[i];
                    var proj = new Project
                    {
                        JobSeekerId = jobSeeker.Id,
                        Title = !string.IsNullOrWhiteSpace(aiProj.Title) ? aiProj.Title : "Unknown Project",
                        TechnologiesUsed = aiProj.TechnologiesUsed,
                        Description = aiProj.Description,
                        ProjectLink = aiProj.ProjectLink,
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Projects.Add(proj);
                }
            }

            // 6) Skills (hard replace, source tagged "AI" for analytics)
            if (extractedData.SkillIds.Any())
            {
                foreach (var skillId in extractedData.SkillIds.Distinct().Take(15))
                {
                    _context.JobSeekerSkills.Add(new JobSeekerSkill
                    {
                        JobSeekerId = jobSeeker.Id,
                        SkillId = skillId,
                        Source = "AI"
                    });
                }
            }

            // 7) Social accounts
            if (extractedData.SocialAccounts != null &&
                (!string.IsNullOrWhiteSpace(extractedData.SocialAccounts.LinkedIn) ||
                 !string.IsNullOrWhiteSpace(extractedData.SocialAccounts.Github) ||
                 !string.IsNullOrWhiteSpace(extractedData.SocialAccounts.Behance) ||
                 !string.IsNullOrWhiteSpace(extractedData.SocialAccounts.Dribbble) ||
                 !string.IsNullOrWhiteSpace(extractedData.SocialAccounts.PersonalWebsite)))
            {
                _context.SocialAccounts.Add(new SocialAccount
                {
                    JobSeekerId = jobSeeker.Id,
                    LinkedIn = extractedData.SocialAccounts.LinkedIn,
                    Github = extractedData.SocialAccounts.Github,
                    Behance = extractedData.SocialAccounts.Behance,
                    Dribbble = extractedData.SocialAccounts.Dribbble,
                    PersonalWebsite = extractedData.SocialAccounts.PersonalWebsite,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
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
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id);

                if (resume == null)
                {
                    return new ResumeResponseDto
                    {
                        Success = true,
                        Message = "No resume uploaded yet.",
                        Resume = null,
                        CurrentStep = 1
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
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id);

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
                    .FirstOrDefaultAsync(r => r.JobSeekerId == jobSeeker.Id);

                if (resume == null)
                {
                    return ResumeResponseDto.FailureResult("No resume found to delete.");
                }

                // Hard delete the resume
                _context.Resumes.Remove(resume);

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

                _logger.LogInformation("Hard deleted resume {ResumeId} for user {UserId}", resume.Id, userId);

                return new ResumeResponseDto
                {
                    Success = true,
                    Message = "Resume deleted successfully",
                    Resume = null,
                    CurrentStep = 1
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
                .AnyAsync(r => r.JobSeekerId == jobSeeker.Id);
        }

        #region Private Helper Methods

        private string ExtractTextFromFile(string path, string extension)
        {
            if (extension == ".pdf")
            {
                using var document = UglyToad.PdfPig.PdfDocument.Open(path);
                var text = new System.Text.StringBuilder();
                foreach (var page in document.GetPages())
                {
                    text.Append(page.Text);
                    text.Append(" ");
                    
                    try
                    {
                        var annotations = page.GetAnnotations();
                        if (annotations != null)
                        {
                            foreach (var ann in annotations)
                            {
                                if (ann.Type == UglyToad.PdfPig.Annotations.AnnotationType.Link &&
                                    ann.Action != null && 
                                    ann.Action is UglyToad.PdfPig.Actions.UriAction uriAction)
                                {
                                    text.AppendLine($"[Link: {uriAction.Uri}]");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore annotation extraction errors to prevent breaking text extraction
                        _logger.LogWarning(ex, "Failed to extract annotations from PDF page.");
                    }
                }
                return text.ToString();
            }
            else if (extension == ".docx")
            {
                using var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(path, false);
                return wordDoc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
            }
            return string.Empty;
        }

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
                DownloadUrl = "/api/jobseeker/resume/download",
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
