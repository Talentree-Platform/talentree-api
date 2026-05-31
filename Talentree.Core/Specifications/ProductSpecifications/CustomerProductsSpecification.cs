using System;
using System.Linq;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class CustomerProductsSpecification : BaseSpecifications<Product>
    {
        public CustomerProductsSpecification(CustomerProductFilterParams filter)
            : base(p =>
                p.Status == ProductStatus.Approved &&
                p.IsVisible == true &&
                (!filter.CategoryId.HasValue || p.CategoryId == filter.CategoryId.Value) &&
                (!filter.BrandId.HasValue || p.BusinessOwnerProfileId == filter.BrandId.Value) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Search) ||
                    p.Name.ToLower().Contains(filter.Search.ToLower()) ||
                    p.Description.ToLower().Contains(filter.Search.ToLower()) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(filter.Search.ToLower())))
            )
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy)
                {
                    case "priceAsc":
                        AddOrderBy(p => p.Price);
                        break;
                    case "priceDesc":
                        AddOrderByDescending(p => p.Price);
                        break;
                    case "rating":
                        AddOrderByDescending(p => p.AvgRating ?? 0f);
                        break;
                    case "newest":
                        AddOrderByDescending(p => p.CreatedAt);
                        break;
                    case "featured":
                        AddOrderByDescending(p => p.IsFeatured);
                        break;
                    default:
                        AddOrderByDescending(p => p.CreatedAt);
                        break;
                }
            }
            else
            {
                AddOrderByDescending(p => p.CreatedAt);
            }

            ApplyPagination(filter.PageIndex, filter.PageSize);
        }

        public CustomerProductsSpecification(CustomerProductFilterParams filter, bool countOnly)
            : base(p =>
                p.Status == ProductStatus.Approved &&
                p.IsVisible == true &&
                (!filter.CategoryId.HasValue || p.CategoryId == filter.CategoryId.Value) &&
                (!filter.BrandId.HasValue || p.BusinessOwnerProfileId == filter.BrandId.Value) &&
                (!filter.MinPrice.HasValue || p.Price >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || p.Price <= filter.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filter.Search) ||
                    p.Name.ToLower().Contains(filter.Search.ToLower()) ||
                    p.Description.ToLower().Contains(filter.Search.ToLower()) ||
                    (p.Tags != null && p.Tags.ToLower().Contains(filter.Search.ToLower())))
            )
        {
        }
    }
}
