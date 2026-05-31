using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.BusinessOwnerSpecifications
{
    public class BrandsSpecification : BaseSpecifications<BusinessOwnerProfile>
    {
        public BrandsSpecification(BrandFilterParams filter)
            : base(b =>
                b.Status == ApprovalStatus.Approved &&
                (string.IsNullOrEmpty(filter.Category) || b.BusinessCategory.ToLower() == filter.Category.ToLower()) &&
                (string.IsNullOrEmpty(filter.Search) || b.BusinessName.ToLower().Contains(filter.Search.ToLower()) || b.BusinessDescription.ToLower().Contains(filter.Search.ToLower()))
            )
        {
            AddInclude(b => b.User);
            AddInclude(b => b.Products);

            if (filter.SortBy == "name")
            {
                AddOrderBy(b => b.BusinessName);
            }
            else
            {
                AddOrderByDescending(b => b.CreatedAt);
            }

            ApplyPagination(filter.PageIndex, filter.PageSize);
        }

        public BrandsSpecification(BrandFilterParams filter, bool countOnly)
            : base(b =>
                b.Status == ApprovalStatus.Approved &&
                (string.IsNullOrEmpty(filter.Category) || b.BusinessCategory.ToLower() == filter.Category.ToLower()) &&
                (string.IsNullOrEmpty(filter.Search) || b.BusinessName.ToLower().Contains(filter.Search.ToLower()) || b.BusinessDescription.ToLower().Contains(filter.Search.ToLower()))
            )
        {
        }
    }
}
