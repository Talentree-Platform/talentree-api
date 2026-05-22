using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class FeaturedProductsSpecification : BaseSpecifications<Product>
    {
        public FeaturedProductsSpecification(int limit = 8)
            : base(p => p.IsFeatured && p.Status == ProductStatus.Approved && p.IsVisible)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);
            AddOrderBy(p => p.FeaturedOrder);
            ApplyPagination(1, limit);
        }
    }
}
