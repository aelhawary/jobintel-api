using RecruitmentPlatformAPI.Models.Identity;

namespace RecruitmentPlatformAPI.Services.Auth
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        string GeneratePasswordResetToken(string email, int userId, int passwordResetId);
        (bool isValid, string email, int userId, int passwordResetId) ValidatePasswordResetToken(string token);
    }
}
