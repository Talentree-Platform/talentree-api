
namespace Talentree.Core.Enums
{
    /// <summary>
    /// Type of notification
    /// Used to categorize and filter notifications
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// System-wide notifications (maintenance, updates, announcements)
        /// </summary>
        System = 1,

        /// <summary>
        /// Order-related notifications (new order, status change, delivery)
        /// </summary>
        Order = 2,

        /// <summary>
        /// Financial notifications (payout, balance, fees)
        /// </summary>
        Financial = 3,

        /// <summary>
        /// Support ticket notifications (response, resolution)
        /// </summary>
        Support = 4,

        /// <summary>
        /// Product notifications (approval, rejection, low stock)
        /// </summary>
        Product = 5
    }
}