using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.NotificationSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Service for managing notifications
    /// Handles all notification operations: creation, retrieval, preferences
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            UserManager<AppUser> userManager,
            IMapper mapper,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════
        // GET NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            int pageIndex = 1,
            int pageSize = 20,
            NotificationType? type = null,
            bool? isRead = null)
        {
            try
            {
                // Validate pagination
                pageIndex = Math.Max(1, pageIndex);
                pageSize = Math.Clamp(pageSize, 1, 100);

                // ✅ Build specification with filters
                var spec = new GetUserNotificationsSpecification(
                    userId,
                    pageIndex,
                    pageSize,
                    type,
                    isRead);

                // ✅ Get notifications
                var notifications = await _unitOfWork.Repository<Notification>()
                    .GetAllWithSpecificationsAsync(spec);

                // ✅ Get total count for pagination
                var totalCountSpec = new GetUserNotificationsSpecification(
                    userId,
                    1,
                    1000,
                    type,
                    isRead);

                var totalCount = await _unitOfWork.Repository<Notification>()
                    .GetCountWithSpecificationsAsync(totalCountSpec);

                // ✅ Map to DTOs
                var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);

                _logger.LogInformation(
                    "Retrieved {Count} notifications for user {UserId}",
                    notifications.Count, userId);

                // ✅ Return paginated result using factory method
                return Pagination<NotificationDto>.Create(
                    notificationDtos.AsReadOnly(),
                    pageIndex,
                    pageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            try
            {
                // ✅ Use specification to get count
                var spec = new GetUnreadNotificationsSpecification(userId);

                return await _unitOfWork.Repository<Notification>()
                    .GetCountWithSpecificationsAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int notificationId, string userId)
        {
            try
            {
                // ✅ Use specification with ownership check
                var spec = new GetNotificationByIdSpecification(notificationId, userId);

                var notification = await _unitOfWork.Repository<Notification>()
                    .GetByIdWithSpecificationsAsync(spec);

                if (notification == null)
                    throw new NotFoundException($"Notification {notificationId} not found");

                return _mapper.Map<NotificationDto>(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {NotificationId}", notificationId);
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MARK AS READ
        // ═══════════════════════════════════════════════════════════

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            try
            {
                // ✅ Get notification with ownership check
                var spec = new GetNotificationByIdSpecification(notificationId, userId);
                var notification = await _unitOfWork.Repository<Notification>()
                    .GetByIdWithSpecificationsAsync(spec);

                if (notification == null)
                    throw new NotFoundException($"Notification {notificationId} not found");

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;

                    _unitOfWork.Repository<Notification>().Update(notification);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                throw;
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            try
            {
                // ✅ Get all unread notifications
                var spec = new GetUnreadNotificationsSpecification(userId);
                var unreadNotifications = await _unitOfWork.Repository<Notification>()
                    .GetAllWithSpecificationsAsync(spec);

                // ✅ Mark all as read
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Notification>().Update(notification);
                }

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Marked {Count} notifications as read for user {UserId}",
                    unreadNotifications.Count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // DELETE NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        public async Task DeleteNotificationAsync(int notificationId, string userId)
        {
            try
            {
                // ✅ Get notification with ownership check
                var spec = new GetNotificationByIdSpecification(notificationId, userId);
                var notification = await _unitOfWork.Repository<Notification>()
                    .GetByIdWithSpecificationsAsync(spec);

                if (notification == null)
                    throw new NotFoundException($"Notification {notificationId} not found");

                // ✅ Delete
                _unitOfWork.Repository<Notification>().Delete(notification);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Notification {NotificationId} deleted", notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification");
                throw;
            }
        }

        public async Task ClearAllReadAsync(string userId)
        {
            try
            {
                // ✅ Get all read notifications
                var spec = new GetReadNotificationsForCleanupSpecification(userId);
                var readNotifications = await _unitOfWork.Repository<Notification>()
                    .GetAllWithSpecificationsAsync(spec);

                var count = readNotifications.Count;

                // ✅ Delete all read notifications
                foreach (var notification in readNotifications)
                {
                    _unitOfWork.Repository<Notification>().Delete(notification);
                }

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Cleared {Count} read notifications for user {UserId}",
                    count, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing read notifications");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // CREATE NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            try
            {
                // ✅ Check if user wants this notification
                var shouldSend = await ShouldSendNotificationAsync(
                    dto.UserId, dto.Type, dto.Priority);

                if (!shouldSend)
                {
                    _logger.LogInformation(
                        "Notification skipped for user {UserId} due to preferences",
                        dto.UserId);
                    return new NotificationDto();
                }

                // ✅ Create notification entity
                var notification = new Notification
                {
                    UserId = dto.UserId,
                    Type = dto.Type,
                    Title = dto.Title,
                    Message = dto.Message,
                    ActionUrl = dto.ActionUrl,
                    ActionText = dto.ActionText,
                    RelatedEntityType = dto.RelatedEntityType,
                    RelatedEntityId = dto.RelatedEntityId,
                    Priority = dto.Priority,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                // ✅ Save to database
                _unitOfWork.Repository<Notification>().Add(notification);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Notification created for user {UserId}: {Title}",
                    dto.UserId, dto.Title);

                // ✅ Send email if required (fire and forget)
                if (dto.SendEmail)
                {
                    _ = SendNotificationEmailAsync(dto.UserId, dto);
                }

                return _mapper.Map<NotificationDto>(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task CreateBulkNotificationAsync(
            List<string> userIds,
            NotificationType type,
            string title,
            string message,
            string? actionUrl = null,
            string? actionText = null,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            try
            {
                if (!userIds.Any())
                {
                    _logger.LogWarning("Bulk notification called with empty user list");
                    return;
                }

                var now = DateTime.UtcNow;

                // ✅ Create notifications for all users
                var notifications = userIds.Select(userId => new Notification
                {
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Message = message,
                    ActionUrl = actionUrl,
                    ActionText = actionText,
                    Priority = priority,
                    IsRead = false,
                    CreatedAt = now
                }).ToList();

                // ✅ Save all notifications
                foreach (var notification in notifications)
                {
                    _unitOfWork.Repository<Notification>().Add(notification);
                }

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Bulk notification created for {Count} users: {Title}",
                    userIds.Count, title);

                // ✅ Send emails asynchronously (fire and forget)
                var emailTasks = userIds.Select(userId =>
                    SendNotificationEmailAsync(userId, new CreateNotificationDto
                    {
                        UserId = userId,
                        Type = type,
                        Title = title,
                        Message = message,
                        ActionUrl = actionUrl,
                        ActionText = actionText,
                        Priority = priority,
                        SendEmail = true
                    })
                );

                _ = Task.WhenAll(emailTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notification");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PREFERENCES
        // ═══════════════════════════════════════════════════════════

        public async Task<NotificationPreferenceDto> GetMyPreferencesAsync(string userId)
        {
            try
            {
                var spec = new GetNotificationPreferenceByUserIdSpecification(userId);

                var preference = await _unitOfWork.Repository<NotificationPreference>()
                    .GetByIdWithSpecificationsAsync(spec);

                // ✅ Create default preferences if not exists
                if (preference == null)
                {
                    preference = new NotificationPreference
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _unitOfWork.Repository<NotificationPreference>().Add(preference);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogInformation("Default preferences created for user {UserId}", userId);
                }

                return _mapper.Map<NotificationPreferenceDto>(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences");
                throw;
            }
        }

        public async Task<NotificationPreferenceDto> UpdatePreferencesAsync(
            string userId,
            UpdateNotificationPreferenceDto dto)
        {
            try
            {
                var spec = new GetNotificationPreferenceByUserIdSpecification(userId);

                var preference = await _unitOfWork.Repository<NotificationPreference>()
                    .GetByIdWithSpecificationsAsync(spec);

                if (preference == null)
                {
                    preference = new NotificationPreference
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _unitOfWork.Repository<NotificationPreference>().Add(preference);
                }

                // ✅ Update only provided fields (null = don't update)
                if (dto.ReceiveAccountNotifications.HasValue)
                    preference.ReceiveAccountNotifications = dto.ReceiveAccountNotifications.Value;

                if (dto.ReceiveOrderNotifications.HasValue)
                    preference.ReceiveOrderNotifications = dto.ReceiveOrderNotifications.Value;

                if (dto.ReceiveProductNotifications.HasValue)
                    preference.ReceiveProductNotifications = dto.ReceiveProductNotifications.Value;

                if (dto.ReceiveSupportNotifications.HasValue)
                    preference.ReceiveSupportNotifications = dto.ReceiveSupportNotifications.Value;

                if (dto.ReceivePaymentNotifications.HasValue)
                    preference.ReceivePaymentNotifications = dto.ReceivePaymentNotifications.Value;

                if (dto.ReceiveReviewNotifications.HasValue)
                    preference.ReceiveReviewNotifications = dto.ReceiveReviewNotifications.Value;

                if (dto.ReceiveProductionNotifications.HasValue)
                    preference.ReceiveProductionNotifications = dto.ReceiveProductionNotifications.Value;

                if (dto.ReceivePayoutNotifications.HasValue)
                    preference.ReceivePayoutNotifications = dto.ReceivePayoutNotifications.Value;

                if (dto.ReceiveComplaintNotifications.HasValue)
                    preference.ReceiveComplaintNotifications = dto.ReceiveComplaintNotifications.Value;

                if (dto.ReceiveAutoBlockNotifications.HasValue)
                    preference.ReceiveAutoBlockNotifications = dto.ReceiveAutoBlockNotifications.Value;

                if (dto.ReceiveInApp.HasValue)
                    preference.ReceiveInApp = dto.ReceiveInApp.Value;

                if (dto.ReceiveEmail.HasValue)
                    preference.ReceiveEmail = dto.ReceiveEmail.Value;

                if (dto.ReceivePush.HasValue)
                    preference.ReceivePush = dto.ReceivePush.Value;

                if (dto.MinimumPriority.HasValue)
                    preference.MinimumPriority = dto.MinimumPriority.Value;

                if (dto.EnableQuietHours.HasValue)
                    preference.EnableQuietHours = dto.EnableQuietHours.Value;

                if (dto.QuietHoursStart.HasValue)
                    preference.QuietHoursStart = dto.QuietHoursStart.Value;

                if (dto.QuietHoursEnd.HasValue)
                    preference.QuietHoursEnd = dto.QuietHoursEnd.Value;

                preference.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<NotificationPreference>().Update(preference);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Notification preferences updated for user {UserId}", userId);

                return _mapper.Map<NotificationPreferenceDto>(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences");
                throw;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        public async Task<bool> ShouldSendNotificationAsync(
            string userId,
            NotificationType type,
            NotificationPriority priority)
        {
            try
            {
                var preference = await GetMyPreferencesAsync(userId);

                // ✅ Check if user wants in-app notifications
                if (!preference.ReceiveInApp)
                    return false;

                // ✅ Check priority filtering
                if (priority < preference.MinimumPriority)
                    return false;

                // ✅ Check type-specific preference
                var shouldReceive = type switch
                {
                    NotificationType.Account => preference.ReceiveAccountNotifications,
                    NotificationType.Order => preference.ReceiveOrderNotifications,
                    NotificationType.Product => preference.ReceiveProductNotifications,
                    NotificationType.Support => preference.ReceiveSupportNotifications,
                    NotificationType.Payment => preference.ReceivePaymentNotifications,
                    NotificationType.Review => preference.ReceiveReviewNotifications,
                    NotificationType.ProductionRequest => preference.ReceiveProductionNotifications,
                    NotificationType.Payout => preference.ReceivePayoutNotifications,
                    NotificationType.Complaint => preference.ReceiveComplaintNotifications,
                    NotificationType.AutoBlock => preference.ReceiveAutoBlockNotifications,
                    _ => true
                };

                return shouldReceive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking notification preference");
                return true; // Send by default if check fails
            }
        }

        private async Task SendNotificationEmailAsync(string userId, CreateNotificationDto dto)
        {
            try
            {
                var preference = await GetMyPreferencesAsync(userId);

                // ✅ Check if user has enabled email notifications
                if (!preference.ReceiveEmail)
                {
                    _logger.LogInformation("Email notification skipped for user {UserId} - disabled", userId);
                    return;
                }

                // ✅ Check quiet hours
                if (preference.EnableQuietHours && IsInQuietHours(preference))
                {
                    _logger.LogInformation("Email notification deferred for user {UserId} - quiet hours", userId);
                    return;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user?.Email == null)
                {
                    _logger.LogWarning("Cannot send email - user {UserId} has no email", userId);
                    return;
                }

                var emailBody = GenerateEmailTemplate(dto, user.DisplayName);
                await _emailService.SendEmailAsync(user.Email, dto.Title, emailBody, isHtml: true);

                _logger.LogInformation("Notification email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification email to user {UserId}", userId);
                // Don't rethrow - email is non-critical
            }
        }

        private bool IsInQuietHours(NotificationPreferenceDto preference)
        {
            if (!preference.EnableQuietHours ||
                preference.QuietHoursStart == null ||
                preference.QuietHoursEnd == null)
                return false;

            var now = TimeOnly.FromDateTime(DateTime.Now);
            var start = preference.QuietHoursStart.Value;
            var end = preference.QuietHoursEnd.Value;

            // Handle overnight ranges (e.g., 22:00 to 08:00)
            if (start < end)
                return now >= start && now < end;
            else
                return now >= start || now < end;
        }

        private string GenerateEmailTemplate(CreateNotificationDto dto, string? userName)
        {
            var priorityColor = dto.Priority switch
            {
                NotificationPriority.Low => "#4CAF50",
                NotificationPriority.Normal => "#2196F3",
                NotificationPriority.High => "#FF9800",
                NotificationPriority.Urgent => "#f44336",
                _ => "#2196F3"
            };

            var actionButton = string.Empty;
            if (!string.IsNullOrEmpty(dto.ActionUrl))
            {
                var buttonText = dto.ActionText ?? "View Details";
                actionButton = $@"
                    <div style='margin-top: 20px;'>
                        <a href='https://talentree.com{dto.ActionUrl}' 
                           style='display: inline-block; padding: 12px 30px; background-color: {priorityColor}; 
                                  color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            {buttonText}
                        </a>
                    </div>";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: {priorityColor}; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .content {{ padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; border-radius: 0 0 5px 5px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; border-top: 1px solid #ddd; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{dto.Title}</h1>
        </div>
        <div class='content'>
            <p>Hello {userName ?? "User"},</p>
            <p>{dto.Message}</p>
            {actionButton}
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
            <p><a href='https://talentree.com/account/notifications/preferences' style='color: #2196F3; text-decoration: none;'>Manage preferences</a></p>
        </div>
    </div>
</body>
</html>";
        }
    }
}