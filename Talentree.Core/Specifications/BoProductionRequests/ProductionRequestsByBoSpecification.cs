// ══════════════════════════════════════════════════════════════
// PRODUCTION REQUESTS — BO side
// ══════════════════════════════════════════════════════════════
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.BoProductionRequests
{
    /// <summary>
    /// Returns a paginated list of all production requests submitted by a specific BO,
    /// including item lines. Ordered newest-first.
    /// </summary>
    public class ProductionRequestsByBoSpecification : BaseSpecifications<BoProductionRequest>
    {
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="pageIndex">1-based page number.</param>
        /// <param name="pageSize">Number of records per page.</param>
        public ProductionRequestsByBoSpecification(string businessOwnerId, int pageIndex, int pageSize)
            : base(r => r.BusinessOwnerId == businessOwnerId)
        {
            AddInclude(r => r.Items);
            AddOrderByDescending(r => r.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Counts all production requests belonging to a specific BO.
    /// Paired with <see cref="ProductionRequestsByBoSpecification"/> for pagination totals.
    /// </summary>
    public class ProductionRequestsCountByBoSpecification : BaseSpecifications<BoProductionRequest>
    {
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        public ProductionRequestsCountByBoSpecification(string businessOwnerId)
            : base(r => r.BusinessOwnerId == businessOwnerId)
        {
        }
    }

    /// <summary>
    /// Retrieves a single production request by ID scoped to a specific BO.
    /// Loads all item lines with preferred raw material details and the full status history.
    /// The BusinessOwnerId scope prevents a BO from accessing another BO's request.
    /// </summary>
    public class ProductionRequestByIdAndBoSpecification : BaseSpecifications<BoProductionRequest>
    {
        /// <param name="requestId">The request's primary key.</param>
        /// <param name="businessOwnerId">The authenticated BO's identity ID (ownership check).</param>
        public ProductionRequestByIdAndBoSpecification(int requestId, string businessOwnerId)
            : base(r => r.Id == requestId && r.BusinessOwnerId == businessOwnerId)
        {
            AddInclude(r => r.Items);
            AddInclude("Items.PreferredRawMaterial");
            AddInclude(r => r.StatusHistory);
        }
    }

    // ══════════════════════════════════════════════════════════
    // PRODUCTION REQUESTS — Admin side
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a paginated list of all production requests across all BOs,
    /// optionally filtered by status. Used for the Admin inbox view.
    /// Ordered newest-first.
    /// </summary>
    public class AllProductionRequestsSpecification : BaseSpecifications<BoProductionRequest>
    {
        /// <param name="status">Optional status filter — null returns all statuses.</param>
        /// <param name="pageIndex">1-based page number.</param>
        /// <param name="pageSize">Number of records per page.</param>
        public AllProductionRequestsSpecification(
            BoProductionRequestStatus? status, int pageIndex, int pageSize)
            : base(r => status == null || r.Status == status)
        {
            AddInclude(r => r.Items);
            AddOrderByDescending(r => r.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Counts all production requests optionally filtered by status.
    /// Paired with <see cref="AllProductionRequestsSpecification"/> for admin pagination totals.
    /// </summary>
    public class AllProductionRequestsCountSpecification : BaseSpecifications<BoProductionRequest>
    {
        /// <param name="status">Optional status filter — null counts all statuses.</param>
        public AllProductionRequestsCountSpecification(BoProductionRequestStatus? status)
            : base(r => status == null || r.Status == status)
        {
        }
    }

    /// <summary>
    /// Retrieves a single production request by ID for Admin use.
    /// No BusinessOwnerId scope — Admins can access any request.
    /// Loads all item lines with preferred raw material details and the full status history.
    /// </summary>
    public class ProductionRequestByIdForAdminSpecification : BaseSpecifications<BoProductionRequest>
    {
        /// <param name="requestId">The request's primary key.</param>
        public ProductionRequestByIdForAdminSpecification(int requestId)
            : base(r => r.Id == requestId)
        {
            AddInclude(r => r.Items);
            AddInclude("Items.PreferredRawMaterial");
            AddInclude(r => r.StatusHistory);
        }
    }

    /// <summary>
    /// Looks up a BoProductionRequest by Stripe PaymentIntent ID.
    /// Includes StatusHistory so the webhook handler can append a history entry.
    /// No BO scope — webhook has no auth context.
    /// </summary>
    public class ProductionRequestByPaymentIntentSpecification : BaseSpecifications<BoProductionRequest>
    {
        public ProductionRequestByPaymentIntentSpecification(string paymentIntentId)
            : base(r => r.StripePaymentIntentId == paymentIntentId)
        {
            AddInclude(r => r.StatusHistory);
        }
    }
}