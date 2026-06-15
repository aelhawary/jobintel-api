using RecruitmentPlatformAPI.DTOs.Notification;

namespace RecruitmentPlatformAPI.Services.Notification
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(
            int userId,
            string type,
            string title,
            string message,
            int? relatedEntityId = null,
            string? relatedEntityType = null,
            string? senderName = null,
            string? senderPictureUrl = null);

        Task<List<NotificationDto>> GetNotificationsAsync(
            int userId,
            bool unreadOnly = false,
            int page = 1,
            int pageSize = 20);

        Task<int> GetUnreadCountAsync(int userId);

        Task<bool> MarkAsReadAsync(int notificationId, int userId);

        Task MarkAllAsReadAsync(int userId);
    }
}
