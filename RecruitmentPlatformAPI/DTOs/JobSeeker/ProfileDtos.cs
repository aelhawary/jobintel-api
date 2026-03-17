namespace RecruitmentPlatformAPI.DTOs.JobSeeker
{
    /// <summary>
    /// Job title information
    /// </summary>
    public class JobTitleDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        /// <example>12</example>
        public int Id { get; set; }

        /// <summary>
        /// Job title in English
        /// </summary>
        /// <example>Backend Developer</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Category grouping (Technology, Design, Marketing, etc.)
        /// </summary>
        /// <example>Technology</example>
        public string? Category { get; set; }
    }
}
