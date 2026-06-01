using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// In-app notification entity
    /// Stores all notifications for users
    /// Supports filtering, marking as read, and deletion
    /// </summary>
    public class Notification : BaseEntity
    {
        // ═══════════════════════════════════════════════════════════
        // OWNERSHIP
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// User who receives this notification
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to AppUser
        /// </summary>
        public AppUser? User { get; set; }

        // ═══════════════════════════════════════════════════════════
        // NOTIFICATION CONTENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Type of notification (Order, Product, Support, etc)
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Notification title (shown in list)
        /// Max 255 chars - should be short and descriptive
        /// Example: "Order Confirmed", "Product Approved"
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed message content
        /// Can be long and include multiple lines
        /// Shown in detail view and in email
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// URL to navigate to (e.g., "/orders/123", "/support/tickets/456")
        /// Frontend uses this for direct navigation
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Button text for the action
        /// If null, frontend uses default "View" or similar
        /// </summary>
        public string? ActionText { get; set; }

        // ═══════════════════════════════════════════════════════════
        // RELATED ENTITY TRACKING
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Entity type this notification is about
        /// Examples: "Order", "Product", "SupportTicket", "ProductionRequest"
        /// Used for grouping related notifications
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// ID of the related entity
        /// Combined with RelatedEntityType to identify the resource
        /// Example: RelatedEntityType="Order", RelatedEntityId=123
        /// </summary>
        public int? RelatedEntityId { get; set; }

        // ═══════════════════════════════════════════════════════════
        // PRIORITY & STATUS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Priority level (Low, Normal, High, Urgent)
        /// Affects notification styling and user filtering
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// Whether user has read this notification
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// When user marked this as read (null if unread)
        /// </summary>
        public DateTime? ReadAt { get; set; }

        // ═══════════════════════════════════════════════════════════
        // TIMESTAMPS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// When notification was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}