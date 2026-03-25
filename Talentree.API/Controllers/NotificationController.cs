
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "BusinessOwner")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<NotificationDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<NotificationDto>>>> GetMyNotifications(
            [FromQuery] NotificationType? type = null,
            [FromQuery] bool? isRead = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.GetMyNotificationsAsync(
                userId, type, isRead, pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<NotificationDto>>.SuccessResponse(
                data: result,
                message: result.Data.Count == 0
                    ? "No notifications found"
                    : $"Retrieved {result.Data.Count} notification(s)"
            ));
        }

        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(ApiResponse<int>.SuccessResponse(
                data: count,
                message: count == 0
                    ? "No unread notifications"
                    : $"You have {count} unread notification(s)"
            ));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<NotificationDto>>> GetNotificationById(int id)
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.GetNotificationByIdAsync(id, userId);

            return Ok(ApiResponse<NotificationDto>.SuccessResponse(
                data: notification,
                message: "Notification retrieved successfully"
            ));
        }

        [HttpPut("{id:int}/mark-as-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(id, userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Notification marked as read"
            ));
        }

        [HttpPut("mark-all-as-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "All notifications marked as read"
            ));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteNotification(int id)
        {
            var userId = GetCurrentUserId();
            await _notificationService.DeleteNotificationAsync(id, userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Notification deleted successfully"
            ));
        }

        [HttpDelete("clear-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ClearAllRead()
        {
            var userId = GetCurrentUserId();
            await _notificationService.ClearAllReadAsync(userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "All read notifications cleared successfully"
            ));
        }

        [HttpGet("preferences")]
        [ProducesResponseType(typeof(ApiResponse<NotificationPreferenceDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> GetMyPreferences()
        {
            var userId = GetCurrentUserId();
            var preferences = await _notificationService.GetMyPreferencesAsync(userId);

            return Ok(ApiResponse<NotificationPreferenceDto>.SuccessResponse(
                data: preferences,
                message: "Notification preferences retrieved successfully"
            ));
        }

        [HttpPut("preferences")]
        [ProducesResponseType(typeof(ApiResponse<NotificationPreferenceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<NotificationPreferenceDto>>> UpdatePreferences(
            [FromBody] UpdateNotificationPreferenceDto dto)
        {
            var userId = GetCurrentUserId();
            var preferences = await _notificationService.UpdatePreferencesAsync(userId, dto);

            return Ok(ApiResponse<NotificationPreferenceDto>.SuccessResponse(
                data: preferences,
                message: "Notification preferences updated successfully"
            ));
        }

     
    }
}