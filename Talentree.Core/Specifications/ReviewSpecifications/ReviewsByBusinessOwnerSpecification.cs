using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.ReviewSpecifications
{
    public class ReviewsByBusinessOwnerSpecification : BaseSpecifications<ProductReview>
    {
        // Paginated + filtered
        public ReviewsByBusinessOwnerSpecification(int businessOwnerProfileId, ReviewFilterParams filter)
            : base(r =>
                r.Product.BusinessOwnerProfileId == businessOwnerProfileId &&
                (filter.Rating == null || r.Rating == filter.Rating) &&
                (filter.ProductId == null || r.ProductId == filter.ProductId) &&
                (string.IsNullOrEmpty(filter.Search) || r.ReviewText != null &&
                    r.ReviewText.ToLower().Contains(filter.Search.ToLower()))
            )
        {
            AddInclude(r => r.Product);
            AddInclude(r => r.Customer);

            switch (filter.SortBy?.ToLower())
            {
                case "rating":
                    if (filter.SortDescending) AddOrderByDescending(r => r.Rating);
                    else AddOrderBy(r => r.Rating);
                    break;
                default: // date
                    if (filter.SortDescending) AddOrderByDescending(r => r.CreatedAt);
                    else AddOrderBy(r => r.CreatedAt);
                    break;
            }

            ApplyPagination(filter.PageIndex, filter.PageSize);
        }

        // Count only
        public ReviewsByBusinessOwnerSpecification(int businessOwnerProfileId, ReviewFilterParams filter, bool countOnly)
            : base(r =>
                r.Product.BusinessOwnerProfileId == businessOwnerProfileId &&
                (filter.Rating == null || r.Rating == filter.Rating) &&
                (filter.ProductId == null || r.ProductId == filter.ProductId) &&
                (string.IsNullOrEmpty(filter.Search) || r.ReviewText != null &&
                    r.ReviewText.ToLower().Contains(filter.Search.ToLower()))
            )
        {
        }
    }
}