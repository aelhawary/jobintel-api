using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatformAPI.Models.Notification
{
    public class Notification
    {
        public int Id { get; set; }

        /// <summary>
        /// The user who receives this notification.
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Notification type (e.g., "RecruiterContact", "ProfileView").
        /// Stored as string for extensibility.
        /// </summary>
        [Required, MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Short headline for the notification.
        /// </summary>
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Body text with context details.
        /// </summary>
        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Flexible reference to a related entity (job, recruiter, etc.).
        /// No FK constraint — kept as int for simplicity.
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Entity type name (e.g., "Job", "Recruiter").
        /// </summary>
        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Display name of the person who triggered this notification.
        /// Denormalized for display performance.
        /// </summary>
        [MaxLength(150)]
        public string? SenderName { get; set; }

        /// <summary>
        /// Profile picture URL of the sender.
        /// </summary>
        [MaxLength(300)]
        public string? SenderPictureUrl { get; set; }

        /// <summary>
        /// Whether the user has read this notification.
        /// </summary>
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public RecruitmentPlatformAPI.Models.Identity.User User { get; set; } = null!;
    }
}
