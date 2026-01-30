using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Profile;
using RecruitmentPlatformAPI.Services.Profile;

namespace RecruitmentPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/profile/social-accounts")]
    public class SocialAccountsController : ControllerBase
    {
        private readonly ISocialAccountService _socialAccountService;
        private readonly ILogger<SocialAccountsController> _logger;

        public SocialAccountsController(
            ISocialAccountService socialAccountService,
            ILogger<SocialAccountsController> logger)
        {
            _socialAccountService = socialAccountService;
            _logger = logger;
        }

        /// <summary>
        /// Add or update social account links (Step 6 of profile wizard)
        /// </summary>
        
        /// <param name="dto">Social account links (all optional)</param>
        /// <returns>Updated social account information or null if skipped</returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(SocialAccountResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateSocialAccount([FromBody] UpdateSocialAccountDto dto)
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

            var result = await _socialAccountService.UpdateSocialAccountAsync(userId, dto);

            if (!result.Success)
            {
                // Check if it's a forbidden error (not a job seeker)
                if (result.Message.Contains("Only job seekers"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current social account links
        /// </summary>
        /// <returns>Social account information or null if not exists</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(SocialAccountResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSocialAccount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _socialAccountService.GetSocialAccountAsync(userId);

            if (!result.Success)
            {
                // Check if it's a forbidden error (not a job seeker)
                if (result.Message.Contains("Only job seekers"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete all social account links
        /// </summary>
        /// <returns>Success message</returns>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteSocialAccount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new ApiErrorResponse("User not authenticated"));
            }

            var result = await _socialAccountService.DeleteSocialAccountAsync(userId);

            if (!result.Success)
            {
                // Check if it's a forbidden error (not a job seeker)
                if (result.Message?.Contains("Only job seekers") == true)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, result);
                }
                return BadRequest(result);
            }

            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
