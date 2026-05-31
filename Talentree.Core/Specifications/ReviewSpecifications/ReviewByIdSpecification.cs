using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.ReviewSpecifications
{
    public class ReviewByIdSpecification : BaseSpecifications<ProductReview>
    {
        public ReviewByIdSpecification(int reviewId)
            : base(r => r.Id == reviewId)
        {
            AddInclude(r => r.Product);
            AddInclude(r => r.Customer);
        }
    }
}