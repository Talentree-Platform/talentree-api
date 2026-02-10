
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.BusinessOwnerSpecifications
{
    /// <summary>
    /// Specification to get pending business owner applications with pagination
    /// </summary>
    public class PendingBusinessOwnersSpecification : BaseSpecifications<BusinessOwnerProfile>
    {
        public PendingBusinessOwnersSpecification(int pageIndex, int pageSize)
            : base(b => b.Status == ApprovalStatus.Pending && !b.IsDeleted)
        {
            // Include user information
            AddInclude(b => b.User);

            // Order by submission date (oldest first)
            AddOrderBy(b => b.CreatedAt);

            // Apply pagination
            ApplyPagination(pageIndex, pageSize);
        }

        /// <summary>
        /// Constructor for counting total items (no pagination)
        /// </summary>
        public PendingBusinessOwnersSpecification()
            : base(b => b.Status == ApprovalStatus.Pending && !b.IsDeleted)
        {
            // No includes or pagination - just for counting
        }
    }
}