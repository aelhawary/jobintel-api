namespace RecruitmentPlatformAPI.DTOs.Profile
{
/// <summary>
    /// Status information for the profile completion wizard
    /// </summary>
    public class WizardStatusDto
    {
        /// <summary>
        /// Current step number (0-6, where 6 means complete)
        /// </summary>
        /// <example>1</example>
        public int CurrentStep { get; set; }

        /// <summary>
        /// Whether the profile is fully complete (currentStep >= 6)
        /// </summary>
        /// <example>false</example>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Human-readable name of the current step
        /// </summary>
        /// <example>Personal Information</example>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// Array of completed step names
        /// </summary>
        /// <example>["Personal Information"]</example>
        public string[] CompletedSteps { get; set; } = Array.Empty<string>();
    }

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

    /// <summary>
    /// Response for profile save operations
    /// </summary>
    public class ProfileResponseDto
    {
        /// <summary>
        /// Whether the operation succeeded
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable success or error message
        /// </summary>
        /// <example>Personal information saved successfully</example>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Updated profile completion step (0-6)
        /// </summary>
        /// <example>1</example>
        public int ProfileCompletionStep { get; set; }
    }
}
