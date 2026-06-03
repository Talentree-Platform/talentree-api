using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.RefundSpecifications
{
    public class AdminRefundRequestsCountSpecification : BaseSpecifications<RefundRequest>
    {
        public AdminRefundRequestsCountSpecification(RefundStatus? status)
            : base(x => !status.HasValue || x.Status == status)
        {
        }
    }
}
