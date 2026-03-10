
namespace Talentree.Core.Entities
{
    /// <summary>
    /// User's notification preferences
    /// Controls which notifications to receive and how
    /// One record per user
    /// </summary>
    public class NotificationPreference : AuditableEntity
    {
        // ═══════════════════════════════════════════════════════════
        // USER REFERENCE
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// User ID (FK to AspNetUsers)
        /// One preference record per user
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Navigation to user
        /// </summary>
        public AppUser User { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════
        // IN-APP NOTIFICATION PREFERENCES (by type)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Enable system notifications in-app?
        /// </summary>
        public bool EnableSystemNotifications { get; set; } = true;

        /// <summary>
        /// Enable order notifications in-app?
        /// </summary>
        public bool EnableOrderNotifications { get; set; } = true;

        /// <summary>
        /// Enable financial notifications in-app?
        /// </summary>
        public bool EnableFinancialNotifications { get; set; } = true;

        /// <summary>
        /// Enable support notifications in-app?
        /// </summary>
        public bool EnableSupportNotifications { get; set; } = true;

        /// <summary>
        /// Enable product notifications in-app?
        /// </summary>
        public bool EnableProductNotifications { get; set; } = true;

        // ═══════════════════════════════════════════════════════════
        // EMAIL NOTIFICATION PREFERENCES (by type)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Send system notifications via email?
        /// </summary>
        public bool EmailSystemNotifications { get; set; } = false;

        /// <summary>
        /// Send order notifications via email?
        /// </summary>
        public bool EmailOrderNotifications { get; set; } = true;

        /// <summary>
        /// Send financial notifications via email?
        /// </summary>
        public bool EmailFinancialNotifications { get; set; } = true;

        /// <summary>
        /// Send support notifications via email?
        /// </summary>
        public bool EmailSupportNotifications { get; set; } = true;

        /// <summary>
        /// Send product notifications via email?
        /// </summary>
        public bool EmailProductNotifications { get; set; } = true;

        // ═══════════════════════════════════════════════════════════
        // REAL-TIME (PUSH) PREFERENCES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Enable real-time push notifications?
        /// Controls SignalR delivery
        /// </summary>
        public bool EnableRealTimeNotifications { get; set; } = true;

        // ═══════════════════════════════════════════════════════════
        // QUIET HOURS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Enable quiet hours feature?
        /// During quiet hours, non-critical notifications are suppressed
        /// </summary>
        public bool EnableQuietHours { get; set; } = false;

        /// <summary>
        /// Quiet hours start time (24-hour format)
        /// Example: 22:00 (10 PM)
        /// </summary>
        public TimeSpan? QuietHoursStart { get; set; }

        /// <summary>
        /// Quiet hours end time (24-hour format)
        /// Example: 08:00 (8 AM)
        /// </summary>
        public TimeSpan? QuietHoursEnd { get; set; }

        // ═══════════════════════════════════════════════════════════
        // SOUND & UI PREFERENCES
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Play notification sound?
        /// (Frontend uses this)
        /// </summary>
        public bool EnableSound { get; set; } = true;

        /// <summary>
        /// Notification sound identifier
        /// Example: "default", "chime", "bell"
        /// Frontend plays corresponding sound file
        /// </summary>
        public string NotificationSound { get; set; } = "default";
    }
}