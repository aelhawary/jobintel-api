using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.DTOs.Recruiter
{
    /// <summary>
    /// Request DTO for contacting a candidate from the recruiter candidate profile page.
    /// The recruiter composes a message which is sent as a branded JobIntel email to the candidate.
    /// </summary>
    public class ContactCandidateRequestDto
    {
        /// <summary>
        /// The message body the recruiter wants to send to the candidate.
        /// Will be embedded in a branded JobIntel email template.
        /// </summary>
        /// <example>Hi Ahmed, we were impressed by your profile and would love to discuss the Senior Frontend Developer role with you.</example>
        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
        public string Message { get; set; } = string.Empty;
    }
}
