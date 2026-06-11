using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Services.JobSeeker;

namespace RecruitmentPlatformAPI.Controllers.JobSeeker
{
    /// <summary>
    /// API endpoints for managing resume/CV uploads (Step 1 of Profile Wizard)
    /// </summary>
    [ApiController]
    [Route("api/jobseeker/resume")]
    [Produces("application/json")]
    [Authorize(Roles = "JobSeeker")]
    public class ResumeController : BaseApiController
    {
        private readonly IResumeService _resumeService;
        private readonly ILogger<ResumeController> _logger;

        public ResumeController(IResumeService resumeService, ILogger<ResumeController> logger)
        {
            _resumeService = resumeService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a new resume/CV or replace the existing one.
        /// </summary>
        /// <remarks>
        /// STRICT FILE UPLOAD ONLY. This endpoint saves the document to storage and
        /// returns its metadata. It does NOT trigger AI parsing and does NOT touch
        /// any profile data (personal info, experiences, skills, etc.).
        ///
        /// AI extraction is invoked in two places only:
        ///  1. The Profile Wizard (Step 1, fast-track flow).
        ///  2. The explicit "Parse &amp; Auto-Fill Profile" action exposed on the
        ///     JobSeeker Dashboard (<c>POST /api/jobseeker/resume/parse</c>).
        ///
        /// Requirements:
        /// - File must be a PDF or DOCX document
        /// - Maximum file size: 10 MB
        /// - Only one resume per user (uploading a new one replaces the existing file)
        /// </remarks>
        /// <param name="file">The PDF or DOCX file to upload</param>
        /// <returns>Resume information with download URL and ParseStatus = "Pending"</returns>
        /// <response code="200">Resume uploaded successfully</response>
        /// <response code="400">Invalid file or validation error</response>
        /// <response code="401">User not authenticated</response>
        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ResumeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResumeResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit at controller level (matches service validation)
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            _logger.LogInformation("UploadResume endpoint called (file-only mode)");

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning("User not authenticated");
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            if (file == null)
            {
                return BadRequest(ResumeResponseDto.FailureResult("No file provided. Please select a PDF or DOCX file to upload."));
            }

            _logger.LogInformation("User {UserId} uploading resume: {FileName} ({Size} bytes)",
                userId, file.FileName, file.Length);

            var result = await _resumeService.UploadResumeAsync(userId, file);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Trigger AI extraction of the user's currently saved resume and comprehensively
        /// update the profile. This is the "Parse &amp; Auto-Fill" action exposed on the
        /// JobSeeker Dashboard and is fully decoupled from file upload.
        /// </summary>
        /// <remarks>
        /// The endpoint re-reads the file the user uploaded, runs the AI extraction
        /// pipeline, and overwrites every relevant section in a single transaction:
        /// <list type="bullet">
        /// <item>Personal Info (job title, years of experience, country, city, phone, bio, first language)</item>
        /// <item>Experiences (soft-delete old, insert new)</item>
        /// <item>Educations (soft-delete old, insert new)</item>
        /// <item>Projects (soft-delete old, insert new)</item>
        /// <item>Skills (hard-replace; max 25; tagged source = "AI")</item>
        /// <item>Social Accounts (hard-replace)</item>
        /// </list>
        /// The user must have already uploaded a resume via <c>POST /resume/upload</c>
        /// (or via the Profile Wizard).
        /// </remarks>
        /// <returns>Resume information plus the extracted data that was applied</returns>
        /// <response code="200">Resume parsed and profile auto-filled successfully</response>
        /// <response code="400">No resume on file, or AI extraction failed</response>
        /// <response code="401">User not authenticated</response>
        [HttpPost("parse")]
        [Authorize]
        [ProducesResponseType(typeof(ResumeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResumeResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ParseResume()
        {
            _logger.LogInformation("ParseResume endpoint called");

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning("User not authenticated");
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _resumeService.ParseResumeAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get the current user's resume information
        /// </summary>
        /// <remarks>
        /// Returns information about the user's uploaded resume including:
        /// - Original filename
        /// - File size (bytes and human-readable)
        /// - Download URL
        /// - Parse status (for future AI parsing feature)
        /// - Upload timestamps
        /// 
        /// If no resume has been uploaded, returns success with null resume data.
        /// </remarks>
        /// <returns>Resume information or empty response if no resume exists</returns>
        /// <response code="200">Resume information retrieved (may be null if no resume uploaded)</response>
        /// <response code="401">User not authenticated</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ResumeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetResume()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _resumeService.GetResumeAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Download the current user's resume file
        /// </summary>
        /// <remarks>
        /// Returns the actual PDF file for download.
        /// The Content-Disposition header is set to "attachment" so browsers will download the file.
        /// </remarks>
        /// <returns>The PDF file as a downloadable attachment</returns>
        /// <response code="200">PDF file returned successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">No resume found</response>
        [HttpGet("download")]
        [Authorize]
        [Produces("application/pdf")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadResume()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var fileResult = await _resumeService.GetResumeFileAsync(userId);

            if (fileResult == null || fileResult.Value.FileStream == null)
            {
                return NotFound(new ApiErrorResponse("No resume found. Please upload a resume first."));
            }

            var (fileStream, contentType, fileName) = fileResult.Value;

            _logger.LogInformation("User {UserId} downloading resume: {FileName}", userId, fileName);

            return File(fileStream, contentType, fileName);
        }

        /// <summary>
        /// Delete the current user's resume (soft delete)
        /// </summary>
        /// <remarks>
        /// Performs a soft delete of the resume:
        /// - The database record is marked as deleted (IsDeleted = true)
        /// - The physical file is removed from storage
        /// 
        /// After deletion, the user can upload a new resume.
        /// </remarks>
        /// <returns>Success confirmation</returns>
        /// <response code="200">Resume deleted successfully</response>
        /// <response code="400">No resume found to delete</response>
        /// <response code="401">User not authenticated</response>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(typeof(ResumeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResumeResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteResume()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            _logger.LogInformation("User {UserId} deleting resume", userId);

            var result = await _resumeService.DeleteResumeAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Check if the current user has an uploaded resume
        /// </summary>
        /// <remarks>
        /// Quick check to determine if the user has completed the resume part of Step 1 of the profile wizard.
        /// Useful for frontend to show/hide the resume step as completed.
        /// </remarks>
        /// <returns>Boolean indicating if resume exists</returns>
        /// <response code="200">Check completed successfully</response>
        /// <response code="401">User not authenticated</response>
        [HttpGet("exists")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckResumeExists()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var hasResume = await _resumeService.HasResumeAsync(userId);

            return Ok(new ApiResponse<bool>(hasResume, hasResume 
                ? "Resume exists" 
                : "No resume uploaded yet"));
        }
    }
}
