namespace RecruitmentPlatformAPI.Enums
{
    /// <summary>
    /// String constants for notification types.
    /// Not an enum — the Notification.Type field is a string for extensibility.
    /// </summary>
    public static class NotificationType
    {
        public const string RecruiterContact = "RecruiterContact";
        public const string ProfileView = "ProfileView";
        public const string ApplicationUpdate = "ApplicationUpdate";
    }
}
