
namespace Talentree.Service.DTOs.Notification
{
    public class NotificationPreferenceDto
    {
        // In-app notifications
        public bool EnableSystemNotifications { get; set; }
        public bool EnableOrderNotifications { get; set; }
        public bool EnableFinancialNotifications { get; set; }
        public bool EnableSupportNotifications { get; set; }
        public bool EnableProductNotifications { get; set; }

        // Email notifications
        public bool EmailSystemNotifications { get; set; }
        public bool EmailOrderNotifications { get; set; }
        public bool EmailFinancialNotifications { get; set; }
        public bool EmailSupportNotifications { get; set; }
        public bool EmailProductNotifications { get; set; }

        // Real-time
        public bool EnableRealTimeNotifications { get; set; }

        // Quiet hours
        public bool EnableQuietHours { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }

        // Sound
        public string NotificationSound { get; set; } = "default";
        public bool EnableSound { get; set; }
    }
}