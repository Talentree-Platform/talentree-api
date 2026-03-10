
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// Represents a notification sent to a user
    /// Supports in-app, email, and real-time delivery via SignalR
    /// </summary>
    public class Notification : AuditableEntity
    {
        // ═══════════════════════════════════════════════════════════
        // RECIPIENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// User ID who receives this notification
        /// FK to AspNetUsers.Id
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to user
        /// </summary>
        public AppUser User { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════
        // CONTENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Notification type (System, Order, Financial, Support, Product)
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Short notification title
        /// Example: "New Order Received", "Product Approved"
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Full notification message
        /// Example: "You received a new order #12345 for EGP 500.00"
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// Optional action URL (where to navigate when clicked)
        /// Example: "/orders/12345", "/products/456"
        /// </summary>
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Optional action button text
        /// Example: "View Order", "Review Product", "Respond"
        /// </summary>
        public string? ActionText { get; set; }

        // ═══════════════════════════════════════════════════════════
        // METADATA
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Related entity type (for reference)
        /// Example: "Order", "Product", "SupportTicket"
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Related entity ID (for reference)
        /// Example: Order ID, Product ID, etc.
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Additional data in JSON format
        /// Example: { "orderId": 123, "amount": 500.00, "customerName": "Ahmed" }
        /// </summary>
        public string? Data { get; set; }

        // ═══════════════════════════════════════════════════════════
        // STATUS & PRIORITY
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Has user read this notification?
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// When notification was read
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Priority level (Low, Normal, High, Critical)
        /// Critical bypasses quiet hours
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        // ═══════════════════════════════════════════════════════════
        // DELIVERY TRACKING
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Was email notification sent?
        /// </summary>
        public bool EmailSent { get; set; } = false;

        /// <summary>
        /// When email was sent
        /// </summary>
        public DateTime? EmailSentAt { get; set; }

        /// <summary>
        /// Was real-time notification sent via SignalR?
        /// </summary>
        public bool RealTimeSent { get; set; } = false;

        /// <summary>
        /// When real-time notification was sent
        /// </summary>
        public DateTime? RealTimeSentAt { get; set; }

        /// <summary>
        /// Optional expiration date (auto-delete after)
        /// Example: Delete system notifications after 30 days
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

      
    }
}