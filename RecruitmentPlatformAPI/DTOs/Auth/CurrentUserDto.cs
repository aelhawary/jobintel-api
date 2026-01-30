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
