namespace RecruitmentPlatformAPI.DTOs.Auth
{
/// <summary>
    /// Current user information extracted from JWT token
    /// </summary>
    public class CurrentUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        /// <summary>
        /// Profile wizard completion step. Job Seekers: 0 = not started, 1-3 = in progress, 4 = complete. Recruiters: 0 = not started, 1 = complete.
        /// Note: This value is a snapshot from login time. Use the wizard-status endpoint for real-time data.
        /// </summary>
        public int ProfileCompletionStep { get; set; }
    }

    /// <summary>
    /// Response wrapper for current user endpoint
    /// </summary>
    public class CurrentUserResponse
    {
        public bool Success { get; set; }
        public CurrentUserDto User { get; set; } = null!;
    }
}
