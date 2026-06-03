using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.RefundSpecifications
{
    public class AdminRefundRequestsSpecification : BaseSpecifications<RefundRequest>
    {
        public AdminRefundRequestsSpecification(RefundStatus? status, int pageIndex, int pageSize)
            : base(x => !status.HasValue || x.Status == status)
        {
            AddInclude(x => x.Order);
            AddInclude(x => x.OrderItem);
            
            AddOrderByDescending(x => x.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }
}
