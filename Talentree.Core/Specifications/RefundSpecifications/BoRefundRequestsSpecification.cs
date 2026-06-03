using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.RefundSpecifications
{
    public class BoRefundRequestsSpecification : BaseSpecifications<RefundRequest>
    {
        public BoRefundRequestsSpecification(string boId, RefundStatus? status, int pageIndex, int pageSize)
            : base(x => x.BusinessOwnerId == boId && (!status.HasValue || x.Status == status))
        {
            AddInclude(x => x.Order);
            AddInclude(x => x.OrderItem);
            
            AddOrderByDescending(x => x.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
        
        public BoRefundRequestsSpecification(int id, string boId)
            : base(x => x.Id == id && x.BusinessOwnerId == boId)
        {
            AddInclude(x => x.Order);
            AddInclude(x => x.OrderItem);
        }
    }
}
