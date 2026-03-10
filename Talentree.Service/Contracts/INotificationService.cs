
using Talentree.Core.Enums;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Contracts
{
    public interface INotificationService
    {
        Task<Pagination<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            NotificationType? type = null,
            bool? isRead = null,
            int pageIndex = 1,
            int pageSize = 20);

        Task<int> GetUnreadCountAsync(string userId);

        Task<NotificationDto> GetNotificationByIdAsync(int notificationId, string userId);

        Task MarkAsReadAsync(int notificationId, string userId);

        Task MarkAllAsReadAsync(string userId);

        Task DeleteNotificationAsync(int notificationId, string userId);

        Task ClearAllReadAsync(string userId);

        // ═══════════════════════════════════════════════════════════
        // CREATE NOTIFICATIONS (Internal use by other services)
        // ═══════════════════════════════════════════════════════════

        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);

        /// <summary>
        /// Send notification to multiple users
        /// </summary>
        Task SendBulkNotificationAsync(
            List<string> userIds,
            NotificationType type,
            string title,
            string message,
            string? actionUrl = null,
            string? actionText = null,
            NotificationPriority priority = NotificationPriority.Normal);

        // ═══════════════════════════════════════════════════════════
        // PREFERENCES
        // ═══════════════════════════════════════════════════════════

        Task<NotificationPreferenceDto> GetMyPreferencesAsync(string userId);

        Task<NotificationPreferenceDto> UpdatePreferencesAsync(
            string userId,
            UpdateNotificationPreferenceDto dto);

        // ═══════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════

        Task<bool> ShouldSendNotificationAsync(
            string userId,
            NotificationType type,
            NotificationPriority priority);
    }
}