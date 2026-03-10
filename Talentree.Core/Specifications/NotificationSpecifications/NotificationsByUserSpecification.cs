
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    
    public class NotificationsByUserSpecification : BaseSpecifications<Notification>
    {
        public NotificationsByUserSpecification(
            string userId,
            NotificationType? type = null,
            bool? isRead = null,
            int? pageIndex = null,
            int? pageSize = null)
            : base(n =>
                n.UserId == userId &&
                (!type.HasValue || n.Type == type.Value) &&
                (!isRead.HasValue || n.IsRead == isRead.Value))
        {
            // Order by newest first
            AddOrderByDescending(n => n.CreatedAt);

            // Apply pagination
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                ApplyPagination(pageIndex.Value ,pageSize.Value);
            }
        }
    }
}