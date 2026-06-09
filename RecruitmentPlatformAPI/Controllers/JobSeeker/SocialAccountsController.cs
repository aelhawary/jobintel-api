using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.JobSeeker;
using RecruitmentPlatformAPI.Services.JobSeeker;

namespace RecruitmentPlatformAPI.Controllers.JobSeeker
{
    [ApiController]
    [Route("api/jobseeker/social-accounts")]
    [Authorize(Roles = "JobSeeker")]
    public class SocialAccountsController : BaseApiController
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
        /// Add or update social account links (Step 4 of profile wizard)
        /// </summary>
        
        /// <param name="dto">Social account links (all optional)</param>
        /// <returns>Updated social account information or null if skipped</returns>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(SocialAccountResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
