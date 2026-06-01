using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Notification
{
    /// <summary>
    /// DTO for updating notification preferences
    /// All properties are optional - only provided values are updated
    /// </summary>
    public class UpdateNotificationPreferenceDto
    {
        // ═══════════════════════════════════════════════════════════
        // NOTIFICATION TYPE PREFERENCES
        // ═══════════════════════════════════════════════════════════

        public bool? ReceiveAccountNotifications { get; set; }
        public bool? ReceiveOrderNotifications { get; set; }
        public bool? ReceiveProductNotifications { get; set; }
        public bool? ReceiveSupportNotifications { get; set; }
        public bool? ReceivePaymentNotifications { get; set; }
        public bool? ReceiveReviewNotifications { get; set; }
        public bool? ReceiveProductionNotifications { get; set; }
        public bool? ReceivePayoutNotifications { get; set; }
        public bool? ReceiveComplaintNotifications { get; set; }
        public bool? ReceiveAutoBlockNotifications { get; set; }

        // ═══════════════════════════════════════════════════════════
        // DELIVERY METHOD PREFERENCES
        // ═══════════════════════════════════════════════════════════

        public bool? ReceiveInApp { get; set; }
        public bool? ReceiveEmail { get; set; }
        public bool? ReceivePush { get; set; }

        // ═══════════════════════════════════════════════════════════
        // FILTERING
        // ═══════════════════════════════════════════════════════════

        public NotificationPriority? MinimumPriority { get; set; }

        // ═══════════════════════════════════════════════════════════
        // QUIET HOURS
        // ═══════════════════════════════════════════════════════════

        public bool? EnableQuietHours { get; set; }
        public TimeOnly? QuietHoursStart { get; set; }
        public TimeOnly? QuietHoursEnd { get; set; }
    }
}