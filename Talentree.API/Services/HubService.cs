using Microsoft.AspNetCore.SignalR;
using Talentree.API.Hubs;
using Talentree.Service.Contracts;

namespace Talentree.API.Services
{
    /// <summary>
    /// Implementation of IHubService
    /// Wraps SignalR HubContext for use by Service layer
    /// </summary>
    public class HubService : IHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<HubService> _logger;

        public HubService(
            IHubContext<NotificationHub> hubContext,
            ILogger<HubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationToUserAsync(string userId, object notification)
        {
            try
            {
                await _hubContext.Clients
                    .User(userId)
                    .SendAsync("ReceiveNotification", notification);

                _logger.LogInformation(
                    "Sent notification to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send notification to user {UserId}", userId);
            }
        }

        public async Task UpdateUnreadCountAsync(string userId, int count)
        {
            try
            {
                await _hubContext.Clients
                    .User(userId)
                    .SendAsync("UnreadCountUpdated", count);

                _logger.LogInformation(
                    "Updated unread count for user {UserId}: {Count}", userId, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update unread count for user {UserId}", userId);
            }
        }

        public async Task SendNotificationToUsersAsync(List<string> userIds, object notification)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    await _hubContext.Clients
                        .User(userId)
                        .SendAsync("ReceiveNotification", notification);
                }

                _logger.LogInformation(
                    "Sent notification to {Count} users", userIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send bulk notification");
            }
        }

        public async Task SendToAllAsync(string method, object data)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(method, data);

                _logger.LogInformation(
                    "Broadcasted {Method} to all connected clients", method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to broadcast to all clients");
            }
        }
    }
}