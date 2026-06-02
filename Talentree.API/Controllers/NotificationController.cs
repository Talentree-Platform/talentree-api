using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Notification management endpoints
    /// Allows users to view, manage, and configure their notifications
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════
        // SPECIFIC ROUTES FIRST (to avoid conflicts)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get count of unread notifications
        /// Useful for badge showing unread count
        /// </summary>
        /// <returns>Count of unread notifications</returns>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} requested unread count", userId);

                var count = await _notificationService.GetUnreadCountAsync(userId);

                return Ok(ApiResponse<int>.SuccessResponse(
                    data: count,
                    message: $"You have {count} unread notification{(count != 1 ? "s" : "")}"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                throw;
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// Bulk operation for user convenience
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPut("mark-all-as-read")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<string>>> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} marked all notifications as read", userId);

                await _notificationService.MarkAllAsReadAsync(userId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    data: "All notifications marked as read",
                    message: "Success"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw;
            }
        }

        /// <summary>
        /// Clear all read notifications (cleanup)
        /// Removes all read notifications from user's list
        /// </summary>
        /// <returns>Success message</returns>
        [HttpDelete("clear-all-read")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<string>>> ClearAllRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} cleared all read notifications", userId);

                await _notificationService.ClearAllReadAsync(userId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    data: "All read notifications cleared",
                    message: "Success"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing read notifications");
                throw;
            }
        }

        /// <summary>
        /// Get current user's notification preferences
        /// Returns all preference settings
        /// </summary>
        /// <returns>User's notification preferences</returns>
        [HttpGet("preferences")]
        [ProducesResponseType(typeof(ApiResponse<NotificationPreferenceDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> GetMyPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} requested preferences", userId);

                var preferences = await _notificationService.GetMyPreferencesAsync(userId);

                return Ok(ApiResponse<NotificationPreferenceDto>.SuccessResponse(
                    data: preferences,
                    message: "Preferences retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences");
                throw;
            }
        }

        /// <summary>
        /// Update notification preferences
        /// Only provided fields are updated
        /// </summary>
        /// <param name="dto">Preferences to update</param>
        /// <returns>Updated preferences</returns>
        [HttpPut("preferences")]
        [ProducesResponseType(typeof(ApiResponse<NotificationPreferenceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> UpdatePreferences(
            [FromBody] UpdateNotificationPreferenceDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} updated preferences", userId);

                var updatedPreferences = await _notificationService.UpdatePreferencesAsync(userId, dto);

                return Ok(ApiResponse<NotificationPreferenceDto>.SuccessResponse(
                    data: updatedPreferences,
                    message: "Preferences updated successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preferences");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // GENERIC ROUTES (with {id})
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get paginated notifications for current user
        /// Supports filtering by type and read status
        /// </summary>
        /// <param name="pageIndex">Page number (1-based, default 1)</param>
        /// <param name="pageSize">Items per page (default 20, max 100)</param>
        /// <param name="type">Optional: filter by notification type</param>
        /// <param name="isRead">Optional: filter by read status (true/false)</param>
        /// <returns>Paginated list of notifications</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<NotificationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<Pagination<NotificationDto>>>> GetMyNotifications(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] NotificationType? type = null,
            [FromQuery] bool? isRead = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation(
                    "User {UserId} requested notifications: page {PageIndex}, size {PageSize}, type {Type}, isRead {IsRead}",
                    userId, pageIndex, pageSize, type, isRead);

                var result = await _notificationService.GetMyNotificationsAsync(
                    userId,
                    pageIndex,
                    pageSize,
                    type,
                    isRead);

                return Ok(ApiResponse<Pagination<NotificationDto>>.SuccessResponse(
                    data: result,
                    message: $"Retrieved {result.Data.Count} notifications"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                throw;
            }
        }

        /// <summary>
        /// Get single notification by ID
        /// Verifies ownership before returning
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Notification details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> GetNotificationById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} requested notification {NotificationId}", userId, id);

                var notification = await _notificationService.GetNotificationByIdAsync(id, userId);

                return Ok(ApiResponse<NotificationDto>.SuccessResponse(
                    data: notification,
                    message: "Notification retrieved successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {NotificationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Mark single notification as read
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Success message</returns>
        [HttpPut("{id}/mark-as-read")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> MarkAsRead(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, id);

                await _notificationService.MarkAsReadAsync(id, userId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    data: "Notification marked as read",
                    message: "Success"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }

        /// <summary>
        /// Delete single notification
        /// </summary>
        /// <param name="id">Notification ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteNotification(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} deleted notification {NotificationId}", userId, id);

                await _notificationService.DeleteNotificationAsync(id, userId);

                return Ok(ApiResponse<string>.SuccessResponse(
                    data: "Notification deleted",
                    message: "Success"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get current user ID from claims
        /// Helper method (from BaseApiController)
        /// </summary>
        /// <returns>Current user ID</returns>
        private string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}