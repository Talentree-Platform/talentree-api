
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    public class NotificationByIdAndUserSpecification : BaseSpecifications<Notification>
    {
        public NotificationByIdAndUserSpecification(int notificationId, string userId)
            : base(n => n.Id == notificationId && n.UserId == userId)
        {
        }
    }
}