using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.MaterialOrders
{
    /// <summary>
    /// Returns a paginated list of all material orders belonging to a specific BO,
    /// including all item lines with their raw material details.
    /// Ordered newest-first.
    /// </summary>
    public class MaterialOrdersByBoSpecification : BaseSpecifications<MaterialOrder>
    {
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="pageIndex">1-based page number.</param>
        /// <param name="pageSize">Number of records per page.</param>
        public MaterialOrdersByBoSpecification(string businessOwnerId, int pageIndex, int pageSize)
            : base(o => o.BusinessOwnerId == businessOwnerId)
        {
            AddInclude(o => o.Items);
            AddInclude("Items.RawMaterial");
            AddOrderByDescending(o => o.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Counts all material orders belonging to a specific BO.
    /// Paired with <see cref="MaterialOrdersByBoSpecification"/> for pagination totals.
    /// </summary>
    public class MaterialOrdersCountByBoSpecification : BaseSpecifications<MaterialOrder>
    {
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        public MaterialOrdersCountByBoSpecification(string businessOwnerId)
            : base(o => o.BusinessOwnerId == businessOwnerId)
        {
        }
    }

    /// <summary>
    /// Retrieves a single material order by ID scoped to a specific BO,
    /// including all item lines with raw material details.
    /// The BusinessOwnerId scope prevents a BO from accessing another BO's order.
    /// </summary>
    public class MaterialOrderByIdAndBoSpecification : BaseSpecifications<MaterialOrder>
    {
        /// <param name="orderId">The order's primary key.</param>
        /// <param name="businessOwnerId">The authenticated BO's identity ID (ownership check).</param>
        public MaterialOrderByIdAndBoSpecification(int orderId, string businessOwnerId)
            : base(o => o.Id == orderId && o.BusinessOwnerId == businessOwnerId)
        {
            AddInclude(o => o.Items);
            AddInclude("Items.RawMaterial");
        }
    }
}