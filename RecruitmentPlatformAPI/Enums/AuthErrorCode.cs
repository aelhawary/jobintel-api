namespace RecruitmentPlatformAPI.Enums
{
    public enum AuthErrorCode
    {
        None = 0,
        InvalidCredentials = 1,
        EmailNotVerified = 2,
        AccountDeactivated = 3,
        AccountLocked = 4,
        OAuthRequired = 5,
        GoogleTokenInvalid = 6,
        GoogleEmailNotVerified = 7,
        GoogleAccountMismatch = 8,
        EmailAlreadyExists = 9,
        InvalidAccountType = 10,
        UserNotFound = 11,
        ResetTokenInvalid = 12,
        ResetTokenExpired = 13
    }
}
