using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.RefundSpecifications
{
    public class BoRefundRequestsCountSpecification : BaseSpecifications<RefundRequest>
    {
        public BoRefundRequestsCountSpecification(string boId, RefundStatus? status)
            : base(x => x.BusinessOwnerId == boId && (!status.HasValue || x.Status == status))
        {
        }
    }
}
