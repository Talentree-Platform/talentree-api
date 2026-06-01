using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// User's notification settings/preferences
    /// Controls which notifications they receive and how
    /// Each user has exactly one preference record
    /// </summary>
    public class NotificationPreference : BaseEntity
    {
        // ═══════════════════════════════════════════════════════════
        // OWNER
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// User who owns these preferences
        /// One-to-one relationship
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation property to AppUser
        /// </summary>
        public AppUser? User { get; set; }

        // ═══════════════════════════════════════════════════════════
        // NOTIFICATION TYPE PREFERENCES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Receive account-related notifications (suspend, ban, approve, etc)
        /// </summary>
        public bool ReceiveAccountNotifications { get; set; } = true;

        /// <summary>
        /// Receive order-related notifications (placed, confirmed, delivered)
        /// </summary>
        public bool ReceiveOrderNotifications { get; set; } = true;

        /// <summary>
        /// Receive product-related notifications (created, approved, rejected)
        /// </summary>
        public bool ReceiveProductNotifications { get; set; } = true;

        /// <summary>
        /// Receive support ticket notifications
        /// </summary>
        public bool ReceiveSupportNotifications { get; set; } = true;

        /// <summary>
        /// Receive payment notifications (success, failure)
        /// </summary>
        public bool ReceivePaymentNotifications { get; set; } = true;

        /// <summary>
        /// Receive review notifications (new review, response to review)
        /// </summary>
        public bool ReceiveReviewNotifications { get; set; } = true;

        /// <summary>
        /// Receive production request notifications
        /// </summary>
        public bool ReceiveProductionNotifications { get; set; } = true;

        /// <summary>
        /// Receive payout notifications
        /// </summary>
        public bool ReceivePayoutNotifications { get; set; } = true;

        /// <summary>
        /// Receive complaint notifications
        /// </summary>
        public bool ReceiveComplaintNotifications { get; set; } = true;

        /// <summary>
        /// Receive auto-block notifications (critical)
        /// </summary>
        public bool ReceiveAutoBlockNotifications { get; set; } = true;

        // ═══════════════════════════════════════════════════════════
        // DELIVERY METHOD PREFERENCES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Receive in-app notifications (dashboard/notification center)
        /// </summary>
        public bool ReceiveInApp { get; set; } = true;

        /// <summary>
        /// Receive email notifications
        /// </summary>
        public bool ReceiveEmail { get; set; } = true;

        /// <summary>
        /// Receive push notifications (future feature)
        /// </summary>
        public bool ReceivePush { get; set; } = false;

        // ═══════════════════════════════════════════════════════════
        // FILTERING OPTIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Minimum priority level to receive
        /// Example: if set to "High", only High and Urgent notifications sent
        /// </summary>
        public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;

        // ═══════════════════════════════════════════════════════════
        // QUIET HOURS (NO EMAIL NOTIFICATIONS DURING THIS TIME)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Whether quiet hours are enabled
        /// </summary>
        public bool EnableQuietHours { get; set; } = false;

        /// <summary>
        /// Start time for quiet hours (e.g., "22:00")
        /// Emails not sent between this time and QuietHoursEnd
        /// </summary>
        public TimeOnly? QuietHoursStart { get; set; }

        /// <summary>
        /// End time for quiet hours (e.g., "08:00")
        /// Can span midnight (e.g., 22:00 to 08:00)
        /// </summary>
        public TimeOnly? QuietHoursEnd { get; set; }

        // ═══════════════════════════════════════════════════════════
        // TIMESTAMPS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// When preferences were created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last time preferences were modified
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}