using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    /// <summary>
    /// Get all unread notifications for a user
    /// Used for marking all as read or getting unread count
    /// </summary>
    public class GetUnreadNotificationsSpecification : BaseSpecifications<Notification>
    {
        public GetUnreadNotificationsSpecification(string userId)
            : base(n => n.UserId == userId && !n.IsRead)
        {
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}