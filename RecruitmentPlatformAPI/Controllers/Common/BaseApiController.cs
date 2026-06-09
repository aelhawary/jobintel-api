using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace RecruitmentPlatformAPI.Controllers.Common
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}
