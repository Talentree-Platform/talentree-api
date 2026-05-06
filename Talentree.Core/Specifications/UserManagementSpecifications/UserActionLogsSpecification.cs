using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class UserActionLogsSpecification : BaseSpecifications<UserActionLog>
    {
        public UserActionLogsSpecification(string userId)
            : base(l => l.UserId == userId)
        {
            AddInclude(l => l.Admin);
            AddOrderByDescending(l => l.ActionDate);
        }
    }
}