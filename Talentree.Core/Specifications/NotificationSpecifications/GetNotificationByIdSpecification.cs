using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    /// <summary>
    /// Get single notification by ID with ownership check
    /// Ensures user can only access their own notifications
    /// </summary>
    public class GetNotificationByIdSpecification : BaseSpecifications<Notification>
    {
        public GetNotificationByIdSpecification(int notificationId, string userId)
            : base(n => n.Id == notificationId && n.UserId == userId)
        {
            AddInclude(n => n.User);
        }
    }
}