namespace RecruitmentPlatformAPI.DTOs.Common
{
    /// <summary>
    /// Status information for the profile completion wizard (shared by Job Seeker and Recruiter)
    /// </summary>
    public class WizardStatusDto
    {
        /// <summary>
        /// Current step number (Job Seekers: 0-4, Recruiters: 0-1)
        /// </summary>
        /// <example>1</example>
        public int CurrentStep { get; set; }

        /// <summary>
        /// Whether the profile is fully complete
        /// </summary>
        /// <example>false</example>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Human-readable name of the current step
        /// </summary>
        /// <example>Personal Info &amp; CV</example>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// Array of completed step names
        /// </summary>
        /// <example>["Personal Information"]</example>
        public string[] CompletedSteps { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Response for profile save and wizard advance operations (shared by Job Seeker and Recruiter)
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
        /// Updated profile completion step
        /// </summary>
        /// <example>1</example>
        public int ProfileCompletionStep { get; set; }
    }
}
