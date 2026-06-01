using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Notification
{
    /// <summary>
    /// Response DTO for notification preferences
    /// Shows user's notification settings
    /// </summary>
    public class NotificationPreferenceDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════
        // NOTIFICATION TYPE PREFERENCES
        // ═══════════════════════════════════════════════════════════

        public bool ReceiveAccountNotifications { get; set; } = true;
        public bool ReceiveOrderNotifications { get; set; } = true;
        public bool ReceiveProductNotifications { get; set; } = true;
        public bool ReceiveSupportNotifications { get; set; } = true;
        public bool ReceivePaymentNotifications { get; set; } = true;
        public bool ReceiveReviewNotifications { get; set; } = true;
        public bool ReceiveProductionNotifications { get; set; } = true;
        public bool ReceivePayoutNotifications { get; set; } = true;
        public bool ReceiveComplaintNotifications { get; set; } = true;
        public bool ReceiveAutoBlockNotifications { get; set; } = true;

        // ═══════════════════════════════════════════════════════════
        // DELIVERY METHOD PREFERENCES
        // ═══════════════════════════════════════════════════════════

        public bool ReceiveInApp { get; set; } = true;
        public bool ReceiveEmail { get; set; } = true;
        public bool ReceivePush { get; set; } = false;

        // ═══════════════════════════════════════════════════════════
        // FILTERING
        // ═══════════════════════════════════════════════════════════

        public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;

        // ═══════════════════════════════════════════════════════════
        // QUIET HOURS
        // ═══════════════════════════════════════════════════════════

        public bool EnableQuietHours { get; set; } = false;
        public TimeOnly? QuietHoursStart { get; set; }
        public TimeOnly? QuietHoursEnd { get; set; }

        // ═══════════════════════════════════════════════════════════
        // TIMESTAMPS
        // ═══════════════════════════════════════════════════════════

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}