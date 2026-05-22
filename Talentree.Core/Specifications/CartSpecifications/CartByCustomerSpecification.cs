using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.CartSpecifications
{
    public class CartByCustomerSpecification : BaseSpecifications<CustomerCart>
    {
        public CartByCustomerSpecification(string customerId)
            : base(c => c.CustomerId == customerId)
        {
            AddInclude("Items.Product.Images");
            AddInclude("Items.Product.BusinessOwner");
        }
    }
}
