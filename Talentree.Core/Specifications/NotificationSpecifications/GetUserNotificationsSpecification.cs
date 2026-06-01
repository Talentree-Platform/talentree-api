using System;
using System.Linq.Expressions;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    /// <summary>
    /// Get paginated notifications for a specific user
    /// Supports filtering by type and read status
    /// </summary>
    public class GetUserNotificationsSpecification : BaseSpecifications<Notification>
    {
        public GetUserNotificationsSpecification(
            string userId,
            int pageIndex = 1,
            int pageSize = 20,
            NotificationType? type = null,
            bool? isRead = null)
            : base(BuildCriteria(userId, type, isRead))
        {
            // Include related user data
            AddInclude(n => n.User);

            // Sort by newest first
            AddOrderByDescending(n => n.CreatedAt);

            // Apply pagination
            ApplyPagination(pageIndex, pageSize);
        }

        /// <summary>
        /// Build dynamic criteria based on filters
        /// </summary>
        private static Expression<Func<Notification, bool>> BuildCriteria(
            string userId,
            NotificationType? type = null,
            bool? isRead = null)
        {
            return n =>
                n.UserId == userId &&
                (type == null || n.Type == type) &&
                (isRead == null || n.IsRead == isRead);
        }
    }
}