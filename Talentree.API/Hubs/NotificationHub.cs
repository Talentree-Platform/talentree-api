
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Talentree.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// Handles WebSocket connections from authenticated users
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════
        // CONNECTION LIFECYCLE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Called when client connects to hub
        /// Adds user to their personal group for targeted messaging
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            // Get current user ID from JWT token
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // Add to user-specific group (for targeted notifications)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

                _logger.LogInformation(
                    "User {UserId} connected to NotificationHub. ConnectionId: {ConnectionId}",
                    userId,
                    Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when client disconnects from hub
        /// Removes user from their group
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");

                _logger.LogInformation(
                    "User {UserId} disconnected from NotificationHub. ConnectionId: {ConnectionId}",
                    userId,
                    Context.ConnectionId);
            }

            if (exception != null)
            {
                _logger.LogError(exception, "Error during disconnection");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ═══════════════════════════════════════════════════════════
        // CLIENT-CALLABLE METHODS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Client calls this to request current unread count
        /// Useful for initial page load
        /// </summary>
        public async Task GetUnreadCount()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // In real implementation, inject INotificationService via DI
            // For now, just send 0
            await Clients.Caller.SendAsync("UnreadCountUpdated", 0);

            _logger.LogInformation(
                "User {UserId} requested unread count",
                userId);
        }

        /// <summary>
        /// Client can ping to test connection
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }
    }
}