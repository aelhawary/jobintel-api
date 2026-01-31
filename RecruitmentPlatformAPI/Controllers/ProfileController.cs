using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Services.Profile;

namespace RecruitmentPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IProfilePictureService _profilePictureService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IProfileService profileService, 
            IProfilePictureService profilePictureService,
            ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _profilePictureService = profilePictureService;
            _logger = logger;
        }

        /// <summary>
        /// Save or update personal information (Step 1 of profile wizard)
        /// </summary>
        /// <param name="dto">Personal information with foreign key IDs (jobTitleId, countryId, languageIds)</param>
        /// <returns>Success response with profile completion step</returns>
        [HttpPost("personal-info")]
        [Authorize]
        [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProfileResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SavePersonalInfo([FromBody] PersonalInfoRequestDto dto)
        {
            _logger.LogInformation("SavePersonalInfo endpoint called");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("User ID from token: {UserId}", userId);
            
            if (userId == 0)
            {
                _logger.LogWarning("User not authenticated");
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            _logger.LogInformation("Calling ProfileService.SavePersonalInfoAsync for user {UserId}", userId);
            var result = await _profileService.SavePersonalInfoAsync(userId, dto);
            
            _logger.LogInformation("SavePersonalInfo result - Success: {Success}, Message: {Message}", result.Success, result.Message);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current personal information with localized reference data
        /// </summary>
        /// <param name="lang">Language code: "en" for English, "ar" for Arabic (default: "en")</param>
        /// <returns>Personal information with both IDs and localized names based on language parameter</returns>
        [HttpGet("personal-info")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PersonalInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPersonalInfo([FromQuery] string lang = "en")
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var personalInfo = await _profileService.GetPersonalInfoAsync(userId, lang);
            
            if (personalInfo is null)
            {
                return NotFound(new ApiErrorResponse("Personal information not found"));
            }

            return Ok(new ApiResponse<PersonalInfoDto>(personalInfo));
        }

        /// <summary>
        /// Get profile completion wizard status (6 steps: Personal Info, Projects, CV Upload, Experience, Education, Social Links)
        /// </summary>
        /// <returns>Current step number, step name, completion status, and list of completed steps</returns>
        [HttpGet("wizard-status")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<WizardStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWizardStatus()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var status = await _profileService.GetWizardStatusAsync(userId);
            return Ok(new ApiResponse<WizardStatusDto>(status));
        }

        /// <summary>
        /// Get list of all available job titles (90 titles across 8 categories: Technology, Design, Marketing, Sales, Finance, HR, Operations, Executive)
        /// </summary>
        /// <returns>List of all active job titles with ID, title, and category</returns>
        [HttpGet("job-titles")]
        [ProducesResponseType(typeof(ApiResponse<List<JobTitleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJobTitles()
        {
            var jobTitles = await _profileService.GetJobTitlesAsync();
            return Ok(new ApiResponse<List<JobTitleDto>>(jobTitles));
        }

        #region Profile Picture Endpoints

        /// <summary>
        /// Upload a profile picture (replaces existing picture if any)
        /// </summary>
        /// <param name="file">Image file (JPEG, PNG, or WebP, max 2MB)</param>
        /// <returns>Upload result with URL</returns>
        [HttpPost("picture")]
        [Authorize]
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

            _logger.LogInformation("Profile picture upload started for user {UserId}, file: {FileName}, size: {Size} bytes",
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
        [Authorize]
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
        [Authorize]
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
                // User has OAuth picture, redirect or return info
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
        [Authorize]
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
                _logger.LogInformation("Profile picture deleted for user {UserId}", userId);
                return Ok(new ApiResponse<bool>(true, "Profile picture deleted successfully"));
            }

            return BadRequest(new ApiErrorResponse("Failed to delete profile picture"));
        }

        /// <summary>
        /// Check if user has a profile picture
        /// </summary>
        /// <returns>Boolean indicating if profile picture exists</returns>
        [HttpGet("picture/exists")]
        [Authorize]
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
