// Talentree.Core/Specifications/ProductSpecifications/PendingProductsSpecification.cs

using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    /// <summary>
    /// Get products pending approval
    /// Includes BusinessOwner, Category, and Images
    /// </summary>
    public class PendingProductsSpecification : BaseSpecifications<Product>
    {
        public PendingProductsSpecification()
            : base(p => p.Status == ProductStatus.PendingApproval)
        {
            AddInclude(p => p.BusinessOwner);
            AddInclude(p => p.Category);
            AddInclude(p => p.Images);

            AddOrderByDescending(p => p.CreatedAt);
        }

        public PendingProductsSpecification(int pageIndex, int pageSize)
            : base(p => p.Status == ProductStatus.PendingApproval)
        {
            AddInclude(p => p.BusinessOwner);
            AddInclude(p => p.Category);
            AddInclude(p => p.Images);
            AddOrderByDescending(p => p.CreatedAt);


            ApplyPagination(pageIndex, pageSize);
        }
    }
}