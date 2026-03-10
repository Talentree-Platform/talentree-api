
using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.NotificationSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Notification service implementation
    /// Handles notification creation, delivery, and preferences
    /// Supports in-app, email, and real-time (SignalR) delivery
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IHubService _hubService;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            IHubService hubService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _hubService = hubService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get user's notifications with filters and pagination
        /// </summary>
        public async Task<Pagination<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            NotificationType? type = null,
            bool? isRead = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            // Validate pagination
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            // Count total matching notifications
            var countSpec = new NotificationsByUserSpecification(userId, type, isRead);
            var totalCount = await _unitOfWork.Repository<Notification>()
                .GetCountWithSpecificationsAsync(countSpec);

            // Get paginated notifications
            var spec = new NotificationsByUserSpecification(
                userId, type, isRead, pageIndex, pageSize);
            var notifications = await _unitOfWork.Repository<Notification>()
                .GetAllWithSpecificationsAsync(spec);

            // Map to DTOs
            var dtos = _mapper.Map<List<NotificationDto>>(notifications);

            return new Pagination<NotificationDto>(
                pageIndex, pageSize, totalCount, dtos);
        }

        /// <summary>
        /// Get unread notification count (for badge display)
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var spec = new UnreadNotificationCountSpecification(userId);
            return await _unitOfWork.Repository<Notification>()
                .GetCountWithSpecificationsAsync(spec);
        }

        /// <summary>
        /// Get single notification by ID with ownership check
        /// </summary>
        public async Task<NotificationDto> GetNotificationByIdAsync(
            int notificationId,
            string userId)
        {
            var spec = new NotificationByIdAndUserSpecification(notificationId, userId);
            var notification = await _unitOfWork.Repository<Notification>()
                .GetByIdWithSpecificationsAsync(spec);

            if (notification == null)
                throw new NotFoundException("Notification not found");

            return _mapper.Map<NotificationDto>(notification);
        }

        // ═══════════════════════════════════════════════════════════
        // MARK AS READ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Mark single notification as read
        /// Updates unread count via SignalR
        /// </summary>
        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var spec = new NotificationByIdAndUserSpecification(notificationId, userId);
            var notification = await _unitOfWork.Repository<Notification>()
                .GetByIdWithSpecificationsAsync(spec);

            if (notification == null)
                throw new NotFoundException("Notification not found");

            // Only update if currently unread
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                _unitOfWork.Repository<Notification>().Update(notification);
                await _unitOfWork.CompleteAsync();

                // Update unread count via SignalR
                await UpdateUnreadCountAsync(userId);
            }
        }

        /// <summary>
        /// Mark all user's notifications as read
        /// </summary>
        public async Task MarkAllAsReadAsync(string userId)
        {
            // Get all unread notifications
            var spec = new NotificationsByUserSpecification(userId, isRead: false);
            var unreadNotifications = await _unitOfWork.Repository<Notification>()
                .GetAllWithSpecificationsAsync(spec);

            if (!unreadNotifications.Any())
                return; // Nothing to update

            // Mark all as read
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.CompleteAsync();

            // Update unread count via SignalR (will be 0)
            await UpdateUnreadCountAsync(userId);
        }

        // ═══════════════════════════════════════════════════════════
        // DELETE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Delete notification (soft delete)
        /// </summary>
        public async Task DeleteNotificationAsync(int notificationId, string userId)
        {
            var spec = new NotificationByIdAndUserSpecification(notificationId, userId);
            var notification = await _unitOfWork.Repository<Notification>()
                .GetByIdWithSpecificationsAsync(spec);

            if (notification == null)
                throw new NotFoundException("Notification not found");

            var wasUnread = !notification.IsRead;

            // Soft delete
            _unitOfWork.Repository<Notification>().Delete(notification);
            await _unitOfWork.CompleteAsync();

            // Update unread count if notification was unread
            if (wasUnread)
            {
                await UpdateUnreadCountAsync(userId);
            }
        }

        /// <summary>
        /// Clear all read notifications (cleanup)
        /// </summary>
        public async Task ClearAllReadAsync(string userId)
        {
            // Get all read notifications
            var spec = new NotificationsByUserSpecification(userId, isRead: true);
            var readNotifications = await _unitOfWork.Repository<Notification>()
                .GetAllWithSpecificationsAsync(spec);

            if (!readNotifications.Any())
                return; // Nothing to clear

            // Delete all read notifications
            foreach (var notification in readNotifications)
            {
                _unitOfWork.Repository<Notification>().Delete(notification);
            }

            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // CREATE NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Create and send notification
        /// Handles in-app, email, and real-time delivery
        /// </summary>
        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            // Check if notification should be sent based on user preferences
            var shouldSend = await ShouldSendNotificationAsync(
                dto.UserId, dto.Type, dto.Priority);

            // Don't send if user disabled this type (unless Critical priority)
            if (!shouldSend && dto.Priority != NotificationPriority.Critical)
            {
                return null!; // Silently skip
            }

            // Create notification entity
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
                Data = dto.Data,
                Priority = dto.Priority,
                ExpiresAt = dto.ExpiresAt,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            _unitOfWork.Repository<Notification>().Add(notification);
            await _unitOfWork.CompleteAsync();

            // Map to DTO
            var notificationDto = _mapper.Map<NotificationDto>(notification);

            // ═══════════════════════════════════════════════════════════
            //  SEND VIA SIGNALR (Real-Time)
            // ═══════════════════════════════════════════════════════════

            await SendRealTimeNotificationAsync(dto.UserId, notificationDto);

            // Mark as sent
            notification.RealTimeSent = true;
            notification.RealTimeSentAt = DateTime.UtcNow;
            _unitOfWork.Repository<Notification>().Update(notification);
            await _unitOfWork.CompleteAsync();

            // ═══════════════════════════════════════════════════════════
            //  SEND EMAIL (if requested)
            // ═══════════════════════════════════════════════════════════

            if (dto.SendEmail)
            {
                await SendEmailNotificationAsync(notification);
            }

            return notificationDto;
        }

        /// <summary>
        /// Send notification to multiple users (bulk)
        /// </summary>
        public async Task SendBulkNotificationAsync(
            List<string> userIds,
            NotificationType type,
            string title,
            string message,
            string? actionUrl = null,
            string? actionText = null,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            // Create notification for each user
            var tasks = userIds.Select(userId =>
            {
                var dto = new CreateNotificationDto
                {
                    UserId = userId,
                    Type = type,
                    Title = title,
                    Message = message,
                    ActionUrl = actionUrl,
                    ActionText = actionText,
                    Priority = priority,
                    SendEmail = false // Usually don't email bulk notifications
                };
                return CreateNotificationAsync(dto);
            });

            // Send all in parallel
            await Task.WhenAll(tasks);
        }

        // ═══════════════════════════════════════════════════════════
        // ⭐ REAL-TIME NOTIFICATION (SignalR)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Send notification to user via SignalR
        /// </summary>
        private async Task SendRealTimeNotificationAsync(
            string userId,
            NotificationDto notification)
        {
            try
            {
                // Check if user enabled real-time notifications
                var preferences = await GetPreferencesAsync(userId);
                if (!preferences.EnableRealTimeNotifications)
                    return; // User disabled real-time

                // Send notification via SignalR
                await _hubService.SendNotificationToUserAsync(userId, notification);

                // Also update unread count
                await UpdateUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                // Log but don't throw (notification is saved in DB anyway)
                Console.WriteLine($" Real-time notification failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Update unread count badge via SignalR
        /// </summary>
        private async Task UpdateUnreadCountAsync(string userId)
        {
            try
            {
                var unreadCount = await GetUnreadCountAsync(userId);
                await _hubService.UpdateUnreadCountAsync(userId, unreadCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unread count update failed: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // 📧 EMAIL NOTIFICATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Send notification via email
        /// </summary>
        private async Task SendEmailNotificationAsync(Notification notification)
        {
            try
            {
                // Get user preferences
                var preferences = await GetPreferencesAsync(notification.UserId);

                // Check if email enabled for this notification type
                bool shouldEmail = notification.Type switch
                {
                    NotificationType.System => preferences.EmailSystemNotifications,
                    NotificationType.Order => preferences.EmailOrderNotifications,
                    NotificationType.Financial => preferences.EmailFinancialNotifications,
                    NotificationType.Support => preferences.EmailSupportNotifications,
                    NotificationType.Product => preferences.EmailProductNotifications,
                    _ => false
                };

                // Critical notifications always sent via email
                if (!shouldEmail && notification.Priority != NotificationPriority.Critical)
                    return;

                // Get user email
                var user = await _unitOfWork.Repository<AppUser>()
                    .GetByIdAsync(notification.UserId);

                if (user?.Email == null)
                    return;

                // Build email
                var subject = $"[Talentree] {notification.Title}";
                var body = BuildEmailBody(notification);

                // Send email
                await _emailService.SendEmailAsync(user.Email, subject, body);

                // Mark as sent
                notification.EmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                _unitOfWork.Repository<Notification>().Update(notification);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                // Log but don't throw
                Console.WriteLine($" Email notification failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Build HTML email body
        /// </summary>
        private string BuildEmailBody(Notification notification)
        {
            // Priority badge
            var priorityBadge = notification.Priority switch
            {
                NotificationPriority.Critical =>
                    "<span style='background: #dc3545; color: white; padding: 4px 12px; border-radius: 4px; font-size: 12px; font-weight: bold;'>🚨 URGENT</span>",
                NotificationPriority.High =>
                    "<span style='background: #ffc107; color: black; padding: 4px 12px; border-radius: 4px; font-size: 12px; font-weight: bold;'>⚠️ HIGH PRIORITY</span>",
                _ => ""
            };

            // Action button
            var actionButton = !string.IsNullOrEmpty(notification.ActionUrl)
                ? $@"
                <div style='margin: 30px 0; text-align: center;'>
                    <a href='https://talentree.com{notification.ActionUrl}' 
                       style='background: #007bff; 
                              color: white; 
                              padding: 12px 30px; 
                              text-decoration: none; 
                              border-radius: 6px; 
                              display: inline-block;
                              font-weight: bold;'>
                        {notification.ActionText ?? "View Details"}
                    </a>
                </div>"
                : "";

            // Type badge
            var typeBadge = notification.Type switch
            {
                NotificationType.Order => "📦 Order",
                NotificationType.Product => "🛍️ Product",
                NotificationType.Financial => "💰 Financial",
                NotificationType.Support => "💬 Support",
                NotificationType.System => "⚙️ System",
                _ => ""
            };

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{notification.Title}</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f5f5f5;'>
    <div style='max-width: 600px; margin: 40px auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center;'>
            <h1 style='margin: 0; font-size: 28px; font-weight: 600;'>Talentree</h1>
            <p style='margin: 10px 0 0 0; font-size: 14px; opacity: 0.9;'>Egyptian Women Empowerment Platform</p>
        </div>

        <!-- Content -->
        <div style='padding: 40px 30px;'>
            
            <!-- Badges -->
            <div style='margin-bottom: 20px;'>
                {priorityBadge}
                {(string.IsNullOrEmpty(priorityBadge) ? "" : "&nbsp;&nbsp;")}
                <span style='background: #f0f0f0; color: #333; padding: 4px 12px; border-radius: 4px; font-size: 12px;'>{typeBadge}</span>
            </div>

            <!-- Title -->
            <h2 style='margin: 0 0 20px 0; color: #333; font-size: 24px; font-weight: 600;'>
                {notification.Title}
            </h2>

            <!-- Message -->
            <div style='color: #666; font-size: 16px; line-height: 1.6; margin-bottom: 20px;'>
                {notification.Message}
            </div>

            <!-- Action Button -->
            {actionButton}

            <!-- Divider -->
            <div style='border-top: 1px solid #e0e0e0; margin: 30px 0;'></div>

            <!-- Metadata -->
            <div style='font-size: 13px; color: #999;'>
                <p style='margin: 5px 0;'>
                    📅 {DateTime.UtcNow:MMMM dd, yyyy} at {DateTime.UtcNow:HH:mm} UTC
                </p>
            </div>
        </div>

        <!-- Footer -->
        <div style='background: #f9f9f9; padding: 30px; text-align: center; border-top: 1px solid #e0e0e0;'>
            <p style='margin: 0 0 15px 0; color: #666; font-size: 14px;'>
                You received this email because you have enabled email notifications for <strong>{notification.Type}</strong>.
            </p>
            <p style='margin: 0 0 15px 0;'>
                <a href='https://talentree.com/settings/notifications' style='color: #667eea; text-decoration: none; font-weight: 500;'>
                    Manage notification preferences
                </a>
            </p>
            <p style='margin: 0; color: #999; font-size: 12px;'>
                &copy; {DateTime.UtcNow.Year} Talentree. All rights reserved.
            </p>
        </div>

    </div>
</body>
</html>";
        }

        // ═══════════════════════════════════════════════════════════
        // PREFERENCES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get user's notification preferences
        /// Creates default preferences if doesn't exist
        /// </summary>
        public async Task<NotificationPreferenceDto> GetMyPreferencesAsync(string userId)
        {
            var preferences = await GetPreferencesAsync(userId);
            return _mapper.Map<NotificationPreferenceDto>(preferences);
        }

        /// <summary>
        /// Update notification preferences (partial update)
        /// </summary>
        public async Task<NotificationPreferenceDto> UpdatePreferencesAsync(
            string userId,
            UpdateNotificationPreferenceDto dto)
        {
            var spec = new PreferenceByUserSpecification(userId);
            var preferences = await _unitOfWork.Repository<NotificationPreference>()
                .GetByIdWithSpecificationsAsync(spec);

            if (preferences == null)
            {
                // Create new preferences with defaults
                preferences = new NotificationPreference
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _unitOfWork.Repository<NotificationPreference>()
                    .Add(preferences);
            }

            // Update only provided values (partial update)

            // In-app notifications
            if (dto.EnableSystemNotifications.HasValue)
                preferences.EnableSystemNotifications = dto.EnableSystemNotifications.Value;

            if (dto.EnableOrderNotifications.HasValue)
                preferences.EnableOrderNotifications = dto.EnableOrderNotifications.Value;

            if (dto.EnableFinancialNotifications.HasValue)
                preferences.EnableFinancialNotifications = dto.EnableFinancialNotifications.Value;

            if (dto.EnableSupportNotifications.HasValue)
                preferences.EnableSupportNotifications = dto.EnableSupportNotifications.Value;

            if (dto.EnableProductNotifications.HasValue)
                preferences.EnableProductNotifications = dto.EnableProductNotifications.Value;

            // Email notifications
            if (dto.EmailSystemNotifications.HasValue)
                preferences.EmailSystemNotifications = dto.EmailSystemNotifications.Value;

            if (dto.EmailOrderNotifications.HasValue)
                preferences.EmailOrderNotifications = dto.EmailOrderNotifications.Value;

            if (dto.EmailFinancialNotifications.HasValue)
                preferences.EmailFinancialNotifications = dto.EmailFinancialNotifications.Value;

            if (dto.EmailSupportNotifications.HasValue)
                preferences.EmailSupportNotifications = dto.EmailSupportNotifications.Value;

            if (dto.EmailProductNotifications.HasValue)
                preferences.EmailProductNotifications = dto.EmailProductNotifications.Value;

            // Real-time
            if (dto.EnableRealTimeNotifications.HasValue)
                preferences.EnableRealTimeNotifications = dto.EnableRealTimeNotifications.Value;

            // Quiet hours
            if (dto.EnableQuietHours.HasValue)
                preferences.EnableQuietHours = dto.EnableQuietHours.Value;

            if (!string.IsNullOrEmpty(dto.QuietHoursStart))
                preferences.QuietHoursStart = TimeSpan.Parse(dto.QuietHoursStart);

            if (!string.IsNullOrEmpty(dto.QuietHoursEnd))
                preferences.QuietHoursEnd = TimeSpan.Parse(dto.QuietHoursEnd);

            // Sound
            if (!string.IsNullOrEmpty(dto.NotificationSound))
                preferences.NotificationSound = dto.NotificationSound;

            if (dto.EnableSound.HasValue)
                preferences.EnableSound = dto.EnableSound.Value;

            preferences.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<NotificationPreference>().Update(preferences);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<NotificationPreferenceDto>(preferences);
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Check if notification should be sent based on user preferences
        /// </summary>
        public async Task<bool> ShouldSendNotificationAsync(
            string userId,
            NotificationType type,
            NotificationPriority priority)
        {
            // Critical notifications ALWAYS sent
            if (priority == NotificationPriority.Critical)
                return true;

            var preferences = await GetPreferencesAsync(userId);

            // Check quiet hours (non-critical notifications only)
            if (preferences.EnableQuietHours && IsInQuietHours(preferences))
                return false;

            // Check type-specific preference
            return type switch
            {
                NotificationType.System => preferences.EnableSystemNotifications,
                NotificationType.Order => preferences.EnableOrderNotifications,
                NotificationType.Financial => preferences.EnableFinancialNotifications,
                NotificationType.Support => preferences.EnableSupportNotifications,
                NotificationType.Product => preferences.EnableProductNotifications,
                _ => true
            };
        }

        /// <summary>
        /// Get user preferences (creates default if doesn't exist)
        /// </summary>
        private async Task<NotificationPreference> GetPreferencesAsync(string userId)
        {
            var spec = new PreferenceByUserSpecification(userId);
            var preferences = await _unitOfWork.Repository<NotificationPreference>()
                .GetByIdWithSpecificationsAsync(spec);

            if (preferences == null)
            {
                // Create default preferences
                preferences = new NotificationPreference
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                    // All other properties use default values from entity
                };

                 _unitOfWork.Repository<NotificationPreference>()
                    .Add(preferences);
                await _unitOfWork.CompleteAsync();
            }

            return preferences;
        }

        /// <summary>
        /// Check if current time is within quiet hours
        /// Handles overnight ranges (e.g., 22:00 - 08:00)
        /// </summary>
        private bool IsInQuietHours(NotificationPreference preferences)
        {
            if (!preferences.QuietHoursStart.HasValue ||
                !preferences.QuietHoursEnd.HasValue)
                return false;

            var now = DateTime.UtcNow.TimeOfDay;
            var start = preferences.QuietHoursStart.Value;
            var end = preferences.QuietHoursEnd.Value;

            if (start < end)
            {
                // Same day range (e.g., 09:00 - 17:00)
                return now >= start && now <= end;
            }
            else
            {
                // Overnight range (e.g., 22:00 - 08:00)
                return now >= start || now <= end;
            }
        }
    }
}