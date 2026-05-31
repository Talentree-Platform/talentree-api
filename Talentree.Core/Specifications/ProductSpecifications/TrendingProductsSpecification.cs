using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class TrendingProductsSpecification : BaseSpecifications<Product>
    {
        public TrendingProductsSpecification(int limit = 10)
            : base(p => p.Status == ProductStatus.Approved && p.IsVisible)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);
            AddOrderByDescending(p => p.PurchaseCount * 10 + p.CartAddCount * 3 + p.ViewCount);
            ApplyPagination(1, limit);
        }
    }
}
