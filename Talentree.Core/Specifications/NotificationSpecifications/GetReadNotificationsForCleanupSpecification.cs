using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    /// <summary>
    /// Get all read notifications for cleanup/deletion
    /// </summary>
    public class GetReadNotificationsForCleanupSpecification : BaseSpecifications<Notification>
    {
        public GetReadNotificationsForCleanupSpecification(string userId)
            : base(n => n.UserId == userId && n.IsRead)
        {
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}