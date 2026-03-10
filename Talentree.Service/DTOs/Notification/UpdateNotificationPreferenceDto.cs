
using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Notification
{
    public class UpdateNotificationPreferenceDto
    {
        // In-app notifications
        public bool? EnableSystemNotifications { get; set; }
        public bool? EnableOrderNotifications { get; set; }
        public bool? EnableFinancialNotifications { get; set; }
        public bool? EnableSupportNotifications { get; set; }
        public bool? EnableProductNotifications { get; set; }

        // Email notifications
        public bool? EmailSystemNotifications { get; set; }
        public bool? EmailOrderNotifications { get; set; }
        public bool? EmailFinancialNotifications { get; set; }
        public bool? EmailSupportNotifications { get; set; }
        public bool? EmailProductNotifications { get; set; }

        // Real-time
        public bool? EnableRealTimeNotifications { get; set; }

        // Quiet hours
        public bool? EnableQuietHours { get; set; }

        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$",
            ErrorMessage = "Invalid time format. Use HH:mm (e.g., 22:00)")]
        public string? QuietHoursStart { get; set; }

        [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$",
            ErrorMessage = "Invalid time format. Use HH:mm (e.g., 08:00)")]
        public string? QuietHoursEnd { get; set; }

        // Sound
        [StringLength(50)]
        public string? NotificationSound { get; set; }

        public bool? EnableSound { get; set; }
    }
}