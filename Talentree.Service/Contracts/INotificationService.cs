using Talentree.Core.Enums;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Service for managing notifications and user preferences
    /// Handles creation, retrieval, and management of notifications
    /// </summary>
    public interface INotificationService
    {
        // ═══════════════════════════════════════════════════════════
        // GET NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get paginated notifications for current user
        /// </summary>
        /// <param name="userId">Current user ID</param>
        /// <param name="pageIndex">Page number (1-based)</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="type">Optional: filter by notification type</param>
        /// <param name="isRead">Optional: filter by read status</param>
        /// <returns>Paginated notification list</returns>
        Task<Pagination<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            int pageIndex = 1,
            int pageSize = 20,
            NotificationType? type = null,
            bool? isRead = null);

        /// <summary>
        /// Get count of unread notifications
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Count of unread notifications</returns>
        Task<int> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Get single notification by ID
        /// Verifies ownership before returning
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <param name="userId">Current user ID (for ownership check)</param>
        /// <returns>Notification details</returns>
        /// <exception cref="NotFoundException">If notification not found</exception>
        /// <exception cref="ForbiddenException">If user doesn't own notification</exception>
        Task<NotificationDto> GetNotificationByIdAsync(int notificationId, string userId);

        // ═══════════════════════════════════════════════════════════
        // MARK AS READ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mark single notification as read
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <param name="userId">Current user ID</param>
        Task MarkAsReadAsync(int notificationId, string userId);

        /// <summary>
        /// Mark all notifications as read for user
        /// </summary>
        /// <param name="userId">User ID</param>
        Task MarkAllAsReadAsync(string userId);

        // ═══════════════════════════════════════════════════════════
        // DELETE NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Delete single notification
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <param name="userId">Current user ID</param>
        Task DeleteNotificationAsync(int notificationId, string userId);

        /// <summary>
        /// Delete all read notifications for user (cleanup)
        /// </summary>
        /// <param name="userId">User ID</param>
        Task ClearAllReadAsync(string userId);

        // ═══════════════════════════════════════════════════════════
        // CREATE NOTIFICATIONS (Internal use)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Create notification for single user
        /// Called by other services when events occur
        /// Respects user preferences
        /// </summary>
        /// <param name="dto">Notification creation data</param>
        /// <returns>Created notification</returns>
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);

        /// <summary>
        /// Create notification for multiple users
        /// Used for bulk notifications (admin notifications, announcements)
        /// </summary>
        /// <param name="userIds">List of user IDs to notify</param>
        /// <param name="type">Notification type</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="actionUrl">Optional: action URL</param>
        /// <param name="actionText">Optional: action button text</param>
        /// <param name="priority">Priority level</param>
        Task CreateBulkNotificationAsync(
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

        /// <summary>
        /// Get user's notification preferences
        /// Creates default preferences if not exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's notification preferences</returns>
        Task<NotificationPreferenceDto> GetMyPreferencesAsync(string userId);

        /// <summary>
        /// Update user's notification preferences
        /// Only updates provided fields (null fields are ignored)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Updated preferences</param>
        /// <returns>Updated preferences</returns>
        Task<NotificationPreferenceDto> UpdatePreferencesAsync(
            string userId,
            UpdateNotificationPreferenceDto dto);

        // ═══════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Check if user wants to receive this notification
        /// Considers preferences, priority filtering, quiet hours, etc.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="type">Notification type</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>True if notification should be sent</returns>
        Task<bool> ShouldSendNotificationAsync(
            string userId,
            NotificationType type,
            NotificationPriority priority);
    }
}