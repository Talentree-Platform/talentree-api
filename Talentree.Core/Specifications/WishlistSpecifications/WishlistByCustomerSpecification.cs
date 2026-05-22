using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.WishlistSpecifications
{
    public class WishlistByCustomerSpecification : BaseSpecifications<CustomerWishlist>
    {
        public WishlistByCustomerSpecification(string customerId)
            : base(w => w.CustomerId == customerId)
        {
            AddInclude("Items.Product.Images");
            AddInclude("Items.Product.BusinessOwner");
        }
    }
}
