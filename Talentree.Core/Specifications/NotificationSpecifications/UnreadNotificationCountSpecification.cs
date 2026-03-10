
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    public class UnreadNotificationCountSpecification : BaseSpecifications<Notification>
    {
        public UnreadNotificationCountSpecification(string userId)
            : base(n => n.UserId == userId && !n.IsRead)
        {
        }
    }
}