using RecruitmentPlatformAPI.Models.Identity;

namespace RecruitmentPlatformAPI.Services.Auth
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}
