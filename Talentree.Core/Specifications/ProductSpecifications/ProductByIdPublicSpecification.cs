using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class ProductByIdPublicSpecification : BaseSpecifications<Product>
    {
        public ProductByIdPublicSpecification(int productId)
            : base(p => p.Id == productId && p.Status == ProductStatus.Approved && p.IsVisible)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);
        }
    }
}
