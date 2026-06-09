using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Services.JobSeeker;

namespace RecruitmentPlatformAPI.Controllers.JobSeeker
{
    /// <summary>
    /// API endpoints for managing certificates (Step 4 of Profile Wizard)
    /// </summary>
    [ApiController]
    [Route("api/jobseeker/certificates")]
    [Authorize(Roles = "JobSeeker")]
    [Produces("application/json")]
    public class CertificatesController : BaseApiController
    {
        private readonly ICertificateService _certificateService;
        private readonly ILogger<CertificatesController> _logger;

        public CertificatesController(ICertificateService certificateService, ILogger<CertificatesController> logger)
        {
            _certificateService = certificateService;
            _logger = logger;
        }

        /// <summary>
        /// Get all certificates for the authenticated job seeker
        /// </summary>
        /// <returns>List of certificates</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(CertificateListResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCertificates()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _certificateService.GetCertificatesAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Get a specific certificate by ID
        /// </summary>
        /// <param name="id">Certificate ID</param>
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCertificateById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _certificateService.GetCertificateByIdAsync(userId, id);
            if (result == null || !result.Success)
                return NotFound(result ?? CertificateResponseDto.FailureResult("Certificate not found"));

            return Ok(result);
        }

        /// <summary>
        /// Add a new certificate with optional file upload
        /// </summary>
        /// <remarks>
        /// Send as multipart/form-data. File is optional — you can add certificate metadata only.
        /// Accepted file types: PDF, DOCX. Max size: 10 MB.
        /// </remarks>
        /// <param name="dto">Certificate metadata</param>
        /// <param name="file">Optional certificate file (PDF/DOCX)</param>
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> AddCertificate([FromForm] CertificateRequestDto dto, IFormFile? file)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _certificateService.AddCertificateAsync(userId, dto, file);
            if (result == null || !result.Success)
                return BadRequest(result ?? CertificateResponseDto.FailureResult("Failed to add certificate"));

            return Ok(result);
        }

        /// <summary>
        /// Update an existing certificate with optional file replacement
        /// </summary>
        /// <param name="id">Certificate ID</param>
        /// <param name="dto">Updated certificate metadata</param>
        /// <param name="file">Optional new file (replaces existing)</param>
        [HttpPut("{id:int}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CertificateResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UpdateCertificate(int id, [FromForm] CertificateRequestDto dto, IFormFile? file)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _certificateService.UpdateCertificateAsync(userId, id, dto, file);
            if (result == null || !result.Success)
            {
                if (result?.Message == "Certificate not found")
                    return NotFound(result);
                return BadRequest(result ?? CertificateResponseDto.FailureResult("Failed to update certificate"));
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a certificate (soft delete)
        /// </summary>
        /// <param name="id">Certificate ID</param>
        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _certificateService.DeleteCertificateAsync(userId, id);
            if (!result)
                return NotFound(new ApiErrorResponse("Certificate not found or already deleted"));

            return Ok(new ApiResponse<bool>(true, "Certificate deleted successfully"));
        }

        /// <summary>
        /// Download a certificate file
        /// </summary>
        /// <param name="id">Certificate ID</param>
        [HttpGet("{id:int}/download")]
        [Authorize]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadCertificateFile(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var fileResult = await _certificateService.DownloadCertificateFileAsync(userId, id);
            if (fileResult == null || fileResult.Value.FileStream == null)
                return NotFound(new ApiErrorResponse("Certificate file not found"));

            var (fileStream, contentType, fileName) = fileResult.Value;
            return File(fileStream, contentType, fileName);
        }
    }
}
