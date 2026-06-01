using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Notification
{
    /// <summary>
    /// Response DTO for notification
    /// Used to return notification data to client
    /// </summary>
    public class NotificationDto
    {
        // ═══════════════════════════════════════════════════════════
        // IDENTITY
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Unique notification ID
        /// </summary>
        public int Id { get; set; }

        // ═══════════════════════════════════════════════════════════
        // TYPE INFORMATION
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Notification type enum value
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Human-readable type name (computed by mapper)
        /// Example: "Order", "Product", "Support"
        /// </summary>
        public string TypeText { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════
        // CONTENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Notification title (shown in list)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed message content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// URL to navigate to when clicked
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Button text for the action
        /// </summary>
        public string? ActionText { get; set; }

        // ═══════════════════════════════════════════════════════════
        // RELATED ENTITY
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Entity type this notification is about
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Related entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        // ═══════════════════════════════════════════════════════════
        // STATUS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Whether notification has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// When notification was marked as read
        /// </summary>
        public DateTime? ReadAt { get; set; }

        // ═══════════════════════════════════════════════════════════
        // PRIORITY
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Priority level enum
        /// </summary>
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// Human-readable priority (computed by mapper)
        /// Example: "Low", "Normal", "High", "Urgent"
        /// </summary>
        public string PriorityText { get; set; } = string.Empty;

        // ═══════════════════════════════════════════════════════════
        // TIMESTAMPS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// When notification was created (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Human-readable time (computed by mapper)
        /// Example: "5 minutes ago", "2 hours ago", "yesterday"
        /// </summary>
        public string TimeAgo { get; set; } = string.Empty;
    }
}