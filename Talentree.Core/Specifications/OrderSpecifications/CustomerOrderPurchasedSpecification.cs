using System.Linq;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class CustomerOrderPurchasedSpecification : BaseSpecifications<CustomerOrder>
    {
        public CustomerOrderPurchasedSpecification(string customerId, int productId)
            : base(o => o.CustomerId == customerId && 
                        o.Status == CustomerOrderStatus.Delivered && 
                        o.Items.Any(i => i.ProductId == productId))
        {
        }
    }
}
