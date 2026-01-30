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
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileService profileService, ILogger<ProfileController> logger)
        {
            _profileService = profileService;
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

        // Helper method to extract user ID from JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
