using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Services.JobSeeker;

namespace RecruitmentPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/jobseeker")]
    [Produces("application/json")]
    [Authorize(Roles = "JobSeeker")]
    public class JobSeekerController : ControllerBase
    {
        private readonly IJobSeekerService _jobSeekerService;
        private readonly IProfilePictureService _profilePictureService;
        private readonly ILogger<JobSeekerController> _logger;

        public JobSeekerController(
            IJobSeekerService jobSeekerService, 
            IProfilePictureService profilePictureService,
            ILogger<JobSeekerController> logger)
        {
            _jobSeekerService = jobSeekerService;
            _profilePictureService = profilePictureService;
            _logger = logger;
        }

        /// <summary>
        /// Save or update personal information (Step 1 of profile wizard)
        /// </summary>
        /// <param name="dto">Personal information with foreign key IDs (jobTitleId, countryId, languageIds)</param>
        /// <returns>Success response with profile completion step</returns>
        [HttpPost("personal-info")]
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

            _logger.LogInformation("Calling JobSeekerService.SavePersonalInfoAsync for user {UserId}", userId);
            var result = await _jobSeekerService.SavePersonalInfoAsync(userId, dto);
            
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

            var personalInfo = await _jobSeekerService.GetPersonalInfoAsync(userId, lang);
            
            if (personalInfo is null)
            {
                return NotFound(new ApiErrorResponse("Personal information not found"));
            }

            return Ok(new ApiResponse<PersonalInfoDto>(personalInfo));
        }

        /// <summary>
        /// Get profile completion wizard status (4 steps: Personal Info, Experience and Education, Projects, Skills and Social and Certificates)
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

            var status = await _jobSeekerService.GetWizardStatusAsync(userId);
            return Ok(new ApiResponse<WizardStatusDto>(status));
        }

        /// <summary>
        /// Explicitly advance the profile completion wizard step. Used when user skips optional fields or explicitly triggers "Next/Finish".
        /// </summary>
        /// <param name="stepNumber">The target step number (1-4)</param>
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

            var result = await _jobSeekerService.AdvanceWizardStepAsync(userId, stepNumber);
            if (!result.Success)
            {
                return BadRequest(new ApiErrorResponse(result.Message));
            }

            return Ok(new ApiResponse<ProfileResponseDto>(result, result.Message));
        }

        /// <summary>
        /// Get list of all available job titles (90 titles across 8 categories: Technology, Design, Marketing, Sales, Finance, HR, Operations, Executive)
        /// </summary>
        /// <returns>List of all active job titles with ID, title, and category</returns>
        [HttpGet("job-titles")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<JobTitleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJobTitles()
        {
            var jobTitles = await _jobSeekerService.GetJobTitlesAsync();
            return Ok(new ApiResponse<List<JobTitleDto>>(jobTitles));
        }

        #region Profile Picture Endpoints

        /// <summary>
        /// Upload a profile picture (replaces existing picture if any)
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
