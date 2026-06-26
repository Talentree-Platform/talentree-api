using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    /// <summary>
    /// FR-AD-11: Low-stock products specification.
    /// Targets approved, visible, non-deleted products whose StockQuantity falls at or below
    /// the supplied threshold (default: 5 units).
    /// Includes BusinessOwner (with User) and Category.
    /// </summary>
    public class LowStockProductsSpecification : BaseSpecifications<Product>
    {
        private const int DefaultThreshold = 5;

        /// <summary>Count-only constructor (no pagination).</summary>
        public LowStockProductsSpecification(int? categoryId = null, int? businessOwnerProfileId = null, int threshold = DefaultThreshold)
            : base(p =>
                p.StockQuantity <= threshold &&
                p.Status == ProductStatus.Approved &&
                !p.IsDeleted &&
                (categoryId == null || p.CategoryId == categoryId) &&
                (businessOwnerProfileId == null || p.BusinessOwnerProfileId == businessOwnerProfileId))
        {
            AddIncludes();
            AddOrderBy(p => p.StockQuantity); // lowest stock first
        }

        /// <summary>Paginated constructor.</summary>
        public LowStockProductsSpecification(int pageIndex, int pageSize,
            int? categoryId = null, int? businessOwnerProfileId = null, int threshold = DefaultThreshold)
            : base(p =>
                p.StockQuantity <= threshold &&
                p.Status == ProductStatus.Approved &&
                !p.IsDeleted &&
                (categoryId == null || p.CategoryId == categoryId) &&
                (businessOwnerProfileId == null || p.BusinessOwnerProfileId == businessOwnerProfileId))
        {
            AddIncludes();
            AddOrderBy(p => p.StockQuantity);
            ApplyPagination(pageIndex, pageSize);
        }

        private void AddIncludes()
        {
            AddInclude(p => p.Category);
            AddInclude("BusinessOwner.User");
        }
    }
}
