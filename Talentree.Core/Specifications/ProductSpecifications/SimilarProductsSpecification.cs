using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class SimilarProductsSpecification : BaseSpecifications<Product>
    {
        public SimilarProductsSpecification(int categoryId, int excludeProductId, int limit = 6)
            : base(p => p.CategoryId == categoryId && p.Id != excludeProductId && p.Status == ProductStatus.Approved && p.IsVisible)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);
            AddOrderByDescending(p => p.CreatedAt);
            ApplyPagination(1, limit);
        }
    }
}
