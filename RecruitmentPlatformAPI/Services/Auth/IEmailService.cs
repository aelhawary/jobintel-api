namespace RecruitmentPlatformAPI.Services.Auth
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string email, string firstName, string verificationCode);
        Task<bool> SendWelcomeEmailAsync(string email, string firstName);
        Task<bool> SendPasswordResetLinkAsync(string email, string firstName, string resetToken);
        Task<bool> SendAccountLockedEmailAsync(string email, string firstName, DateTime lockoutEnd, string resetToken);
        Task<bool> SendWeeklyDigestAsync(string email, string firstName, int searchAppearances, int profileViews, int recommendations);
        Task<bool> SendContactEmailAsync(string candidateEmail, string candidateFirstName, string recruiterEmail, string recruiterFirstName, string recruiterLastName, string recruiterCompany, string jobTitle, string message);
        string GenerateVerificationCode();
        string GenerateSecureToken();
    }
}
