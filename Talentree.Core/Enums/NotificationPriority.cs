
namespace Talentree.Core.Enums
{
    public enum NotificationPriority
    {
        Low = 0,        // Nice to have (system updates, info)
        Normal = 1,     // Standard notifications (orders, products)
        High = 2,       // Important (payments, account actions)
        Urgent = 3      // Critical (bans, auto-blocks, urgent issues)
    }
}