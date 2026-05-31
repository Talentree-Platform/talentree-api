using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class CustomerOrderByPaymentIntentSpecification : BaseSpecifications<CustomerOrder>
    {
        public CustomerOrderByPaymentIntentSpecification(string paymentIntentId)
            : base(o => o.StripePaymentIntentId == paymentIntentId)
        {
            AddInclude("Items.Product.Images");
            AddInclude("Items.Product.BusinessOwner");
            AddInclude("StatusHistory");
        }
    }
}
