using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class ProductsByBusinessOwnerSpecification : BaseSpecifications<Product>
    {
        // With filters + pagination
        public ProductsByBusinessOwnerSpecification(int businessOwnerProfileId, ProductFilterParams filter)
    : base(p =>
        p.BusinessOwnerProfileId == businessOwnerProfileId &&
        (filter.Status == null || p.Status == filter.Status) &&
        (string.IsNullOrEmpty(filter.Search) || p.Name.ToLower().Contains(filter.Search.ToLower()))
    )
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);

            switch (filter.SortBy?.ToLower())
            {
                case "price":
                    if (filter.SortDescending) AddOrderByDescending(p => p.Price);
                    else AddOrderBy(p => p.Price);
                    break;
                case "stock":
                    if (filter.SortDescending) AddOrderByDescending(p => p.StockQuantity);
                    else AddOrderBy(p => p.StockQuantity);
                    break;
                default:
                    AddOrderByDescending(p => p.CreatedAt);
                    break;
            }

            ApplyPagination(filter.PageIndex, filter.PageSize);
        }

        // Count only - no includes, no pagination
        public ProductsByBusinessOwnerSpecification(int businessOwnerProfileId, ProductFilterParams filter, bool countOnly)
    : base(p =>
        p.BusinessOwnerProfileId == businessOwnerProfileId &&
        (filter.Status == null || p.Status == filter.Status) &&
        (string.IsNullOrEmpty(filter.Search) || p.Name.ToLower().Contains(filter.Search.ToLower()))
    )
        {
        }
    }
}
