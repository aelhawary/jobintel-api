namespace RecruitmentPlatformAPI.Services.Auth
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string email, string firstName, string verificationCode);
        Task<bool> SendWelcomeEmailAsync(string email, string firstName);
        Task<bool> SendPasswordResetLinkAsync(string email, string firstName, string resetToken);
        Task<bool> SendAccountLockedEmailAsync(string email, string firstName, DateTime lockoutEnd);
        string GenerateVerificationCode();
        string GenerateSecureToken();
    }
}
