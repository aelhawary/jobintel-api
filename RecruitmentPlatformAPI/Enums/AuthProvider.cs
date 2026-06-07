namespace RecruitmentPlatformAPI.Enums
{
    /// <summary>
    /// Authentication provider used for user registration and login
    /// </summary>
    public enum AuthProvider
    {
        /// <summary>
        /// Email/Password authentication
        /// </summary>
        Email = 1,

        /// <summary>
        /// Google OAuth authentication
        /// </summary>
        Google = 2
    }
}
