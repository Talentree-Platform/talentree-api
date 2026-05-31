using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.PayoutRequests
{
    /// <summary>
    /// Returns paginated payout requests for a BO, newest first.
    /// </summary>
    public class PayoutRequestsByBoSpecification : BaseSpecifications<PayoutRequest>
    {
        public PayoutRequestsByBoSpecification(string businessOwnerId, int pageIndex, int pageSize)
            : base(p => p.BusinessOwnerId == businessOwnerId)
        {
            AddOrderByDescending(p => p.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Counts payout requests for a BO.
    /// </summary>
    public class PayoutRequestsCountByBoSpecification : BaseSpecifications<PayoutRequest>
    {
        public PayoutRequestsCountByBoSpecification(string businessOwnerId)
            : base(p => p.BusinessOwnerId == businessOwnerId)
        {
        }
    }

    /// <summary>
    /// Checks whether a BO has a pending payout request.
    /// Used to enforce the one-pending-at-a-time rule.
    /// </summary>
    public class PendingPayoutByBoSpecification : BaseSpecifications<PayoutRequest>
    {
        public PendingPayoutByBoSpecification(string businessOwnerId)
            : base(p => p.BusinessOwnerId == businessOwnerId
                     && p.Status == PayoutStatus.Pending)
        {
        }
    }

    /// <summary>
    /// Returns all payout requests filtered by status for Admin inbox.
    /// </summary>
    public class AllPayoutRequestsSpecification : BaseSpecifications<PayoutRequest>
    {
        public AllPayoutRequestsSpecification(PayoutStatus? status, int pageIndex, int pageSize)
            : base(p => status == null || p.Status == status)
        {
            AddOrderByDescending(p => p.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>Counts payout requests for Admin, optionally filtered by status.</summary>
    public class AllPayoutRequestsCountSpecification : BaseSpecifications<PayoutRequest>
    {
        public AllPayoutRequestsCountSpecification(PayoutStatus? status)
            : base(p => status == null || p.Status == status)
        {
        }
    }
}