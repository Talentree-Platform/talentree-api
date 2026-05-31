using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.ReviewSpecifications
{
    /// <summary>
    /// Used for analytics — fetches ALL reviews with no pagination
    /// </summary>
    public class AllReviewsByBusinessOwnerSpecification : BaseSpecifications<ProductReview>
    {
        public AllReviewsByBusinessOwnerSpecification(int businessOwnerProfileId)
            : base(r => r.Product.BusinessOwnerProfileId == businessOwnerProfileId)
        {
        }
    }
}