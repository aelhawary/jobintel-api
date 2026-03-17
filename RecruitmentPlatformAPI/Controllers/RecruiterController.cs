using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.DTOs.Recruiter;
using RecruitmentPlatformAPI.Services.JobSeeker;
using RecruitmentPlatformAPI.Services.Recruiter;

namespace RecruitmentPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/recruiter")]
    [Produces("application/json")]
    [Authorize(Roles = "Recruiter")]
    public class RecruiterController : ControllerBase
    {
        private readonly IRecruiterService _recruiterService;
        private readonly IProfilePictureService _profilePictureService;
        private readonly ILogger<RecruiterController> _logger;

        public RecruiterController(
            IRecruiterService recruiterService,
            IProfilePictureService profilePictureService,
            ILogger<RecruiterController> logger)
        {
            _recruiterService = recruiterService;
            _profilePictureService = profilePictureService;
            _logger = logger;
        }

        /// <summary>
        /// Save or update company information (Recruiter profile completion — single step)
        /// </summary>
        /// <param name="dto">Company information (name, size, industry, location, optional website/LinkedIn/description)</param>
        /// <returns>Success response with profile completion step</returns>
        [HttpPost("company-info")]
        [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SaveCompanyInfo([FromBody] RecruiterCompanyInfoRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _recruiterService.SaveCompanyInfoAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current company information for the authenticated recruiter
        /// </summary>
        /// <returns>Company information or 404 if not yet completed</returns>
        [HttpGet("company-info")]
        [ProducesResponseType(typeof(ApiResponse<RecruiterCompanyInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCompanyInfo()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var companyInfo = await _recruiterService.GetCompanyInfoAsync(userId);

            if (companyInfo is null)
            {
                return NotFound(new ApiErrorResponse("Company information not found. Please complete your profile."));
            }

            return Ok(new ApiResponse<RecruiterCompanyInfoDto>(companyInfo));
        }

        /// <summary>
        /// Get recruiter profile completion wizard status (1 step: Company Information)
        /// </summary>
        /// <returns>Current step number, step name, completion status, and list of completed steps</returns>
        [HttpGet("wizard-status")]
        [ProducesResponseType(typeof(ApiResponse<WizardStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWizardStatus()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var status = await _recruiterService.GetWizardStatusAsync(userId);
            return Ok(new ApiResponse<WizardStatusDto>(status));
        }

        /// <summary>
        /// Explicitly advance the profile completion wizard step.
        /// </summary>
        /// <param name="stepNumber">The target step number</param>
        /// <returns>Success response with updated profile completion step</returns>
        [HttpPost("wizard/advance/{stepNumber}")]
        [ProducesResponseType(typeof(ApiResponse<ProfileResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AdvanceWizardStep(int stepNumber)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _recruiterService.AdvanceWizardStepAsync(userId, stepNumber);
            if (!result.Success)
            {
                return BadRequest(new ApiErrorResponse(result.Message));
            }

            return Ok(new ApiResponse<ProfileResponseDto>(result, result.Message));
        }

        /// <summary>
        /// Get list of predefined industry options for the dropdown
        /// </summary>
        /// <returns>List of available industries</returns>
        [HttpGet("industries")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<IndustryDto>>), StatusCodes.Status200OK)]
        public IActionResult GetIndustries()
        {
            var industries = _recruiterService.GetIndustries();
            return Ok(new ApiResponse<List<IndustryDto>>(industries));
        }

        /// <summary>
        /// Get list of predefined company size options for the dropdown
        /// </summary>
        /// <returns>List of available company sizes with labels</returns>
        [HttpGet("company-sizes")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<CompanySizeDto>>), StatusCodes.Status200OK)]
        public IActionResult GetCompanySizes()
        {
            var sizes = _recruiterService.GetCompanySizes();
            return Ok(new ApiResponse<List<CompanySizeDto>>(sizes));
        }

        #region Profile Picture Endpoints

        /// <summary>
        /// Upload a profile picture for the recruiter (replaces existing picture if any)
        /// </summary>
        /// <param name="file">Image file (JPEG, PNG, or WebP, max 2MB)</param>
        /// <returns>Upload result with URL</returns>
        [HttpPost("picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProfilePictureUploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProfilePictureUploadResultDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(2 * 1024 * 1024)] // 2MB limit
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProfilePictureUploadResultDto
                {
                    Success = false,
                    Message = "No file provided"
                });
            }

            _logger.LogInformation("Profile picture upload started for recruiter {UserId}, file: {FileName}, size: {Size} bytes",
                userId, file.FileName, file.Length);

            using var stream = file.OpenReadStream();
            var result = await _profilePictureService.UploadProfilePictureAsync(
                userId, stream, file.FileName, file.ContentType);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get profile picture information (URL, metadata)
        /// </summary>
        /// <returns>Profile picture info with URL and metadata</returns>
        [HttpGet("picture/info")]
        [ProducesResponseType(typeof(ApiResponse<ProfilePictureResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfilePictureInfo()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var pictureInfo = await _profilePictureService.GetProfilePictureAsync(userId);
            return Ok(new ApiResponse<ProfilePictureResponseDto>(pictureInfo));
        }

        /// <summary>
        /// Get the actual profile picture file (for display)
        /// </summary>
        /// <returns>Image file stream</returns>
        [HttpGet("picture")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfilePicture()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var fileResult = await _profilePictureService.GetProfilePictureFileAsync(userId);

            if (fileResult == null)
            {
                return NotFound(new ApiErrorResponse("Profile picture not found"));
            }

            var (stream, contentType, fileName) = fileResult.Value;

            if (stream == null)
            {
                var pictureInfo = await _profilePictureService.GetProfilePictureAsync(userId);
                if (pictureInfo.IsOAuthPicture && !string.IsNullOrEmpty(pictureInfo.Url))
                {
                    return Redirect(pictureInfo.Url);
                }
                return NotFound(new ApiErrorResponse("Profile picture not found"));
            }

            return File(stream, contentType ?? "image/jpeg", fileName);
        }

        /// <summary>
        /// Delete the profile picture
        /// </summary>
        /// <returns>Success status</returns>
        [HttpDelete("picture")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _profilePictureService.DeleteProfilePictureAsync(userId);

            if (result)
            {
                _logger.LogInformation("Profile picture deleted for recruiter {UserId}", userId);
                return Ok(new ApiResponse<bool>(true, "Profile picture deleted successfully"));
            }

            return BadRequest(new ApiErrorResponse("Failed to delete profile picture"));
        }

        /// <summary>
        /// Check if recruiter has a profile picture
        /// </summary>
        /// <returns>Boolean indicating if profile picture exists</returns>
        [HttpGet("picture/exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> HasProfilePicture()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var pictureInfo = await _profilePictureService.GetProfilePictureAsync(userId);
            return Ok(new ApiResponse<bool>(pictureInfo.HasProfilePicture));
        }

        #endregion

        // Helper method to extract user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
