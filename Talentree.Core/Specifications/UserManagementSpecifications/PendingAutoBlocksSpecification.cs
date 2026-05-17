using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class PendingAutoBlocksSpecification : BaseSpecifications<AutoBlockLog>
    {
        public PendingAutoBlocksSpecification()
            : base(l => !l.IsReviewed)
        {
            AddInclude(l => l.User);
            AddOrderByDescending(l => l.BlockedAt);
        }
    }
}