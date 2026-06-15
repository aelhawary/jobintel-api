using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentPlatformAPI.Controllers.Common;
using RecruitmentPlatformAPI.DTOs.Common;
using RecruitmentPlatformAPI.DTOs.Notification;
using RecruitmentPlatformAPI.Services.Notification;

namespace RecruitmentPlatformAPI.Controllers.Notification
{
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get current user's notifications, sorted by newest first.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);
            var notifications = await _notificationService.GetNotificationsAsync(userId, unreadOnly, page, pageSize);
            return Ok(new ApiResponse<List<NotificationDto>>(notifications));
        }

        /// <summary>
        /// Get the count of unread notifications for the current user.
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<UnreadCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new ApiResponse<UnreadCountDto>(new UnreadCountDto { Count = count }));
        }

        /// <summary>
        /// Mark a single notification as read.
        /// </summary>
        [HttpPut("{id}/read")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            var result = await _notificationService.MarkAsReadAsync(id, userId);
            if (!result)
                return NotFound(new ApiErrorResponse("Notification not found."));

            return Ok(new ApiResponse<bool>(true));
        }

        /// <summary>
        /// Mark all notifications as read for the current user.
        /// </summary>
        [HttpPut("read-all")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ApiErrorResponse("User not authenticated"));

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new ApiResponse<bool>(true));
        }
    }
}
