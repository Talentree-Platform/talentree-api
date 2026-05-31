using System;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.ReviewSpecifications
{
    public class ProductReviewsSpecification : BaseSpecifications<ProductReview>
    {
        public ProductReviewsSpecification(int productId, CustomerReviewFilterParams filter)
            : base(r =>
                r.ProductId == productId &&
                (!filter.Rating.HasValue || r.Rating == filter.Rating.Value)
            )
        {
            AddInclude(r => r.Photos);
            AddInclude(r => r.Customer);

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy)
                {
                    case "newest":
                        AddOrderByDescending(r => r.CreatedAt);
                        break;
                    case "helpful":
                        AddOrderByDescending(r => r.HelpfulVotes);
                        break;
                    case "ratingHigh":
                        AddOrderByDescending(r => r.Rating);
                        break;
                    case "ratingLow":
                        AddOrderBy(r => r.Rating);
                        break;
                    default:
                        AddOrderByDescending(r => r.CreatedAt);
                        break;
                }
            }
            else
            {
                AddOrderByDescending(r => r.CreatedAt);
            }

            ApplyPagination(filter.PageIndex, filter.PageSize);
        }

        public ProductReviewsSpecification(int productId, CustomerReviewFilterParams filter, bool countOnly)
            : base(r =>
                r.ProductId == productId &&
                (!filter.Rating.HasValue || r.Rating == filter.Rating.Value)
            )
        {
        }
    }
}
