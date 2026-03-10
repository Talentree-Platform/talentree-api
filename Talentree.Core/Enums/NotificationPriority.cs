
namespace Talentree.Core.Enums
{
    /// <summary>
    /// Priority level of notification
    /// Determines urgency and delivery behavior
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// Low priority - can be ignored or delayed
        /// </summary>
        Low = 1,

        /// <summary>
        /// Normal priority - standard notification
        /// </summary>
        Normal = 2,

        /// <summary>
        /// High priority - should be seen soon
        /// </summary>
        High = 3,

        /// <summary>
        /// Critical - must be seen immediately
        /// Bypasses quiet hours and user preferences
        /// </summary>
        Critical = 4
    }
}