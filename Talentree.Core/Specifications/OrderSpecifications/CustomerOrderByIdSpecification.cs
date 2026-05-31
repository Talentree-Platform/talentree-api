using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class CustomerOrderByIdSpecification : BaseSpecifications<CustomerOrder>
    {
        public CustomerOrderByIdSpecification(int orderId, string customerId)
            : base(o => o.Id == orderId && o.CustomerId == customerId)
        {
            AddInclude("Items.Product.Images");
            AddInclude("StatusHistory");
        }
    }
}
