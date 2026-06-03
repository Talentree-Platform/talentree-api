using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.RefundSpecifications
{
    public class RefundRequestsByItemSpecification : BaseSpecifications<RefundRequest>
    {
        public RefundRequestsByItemSpecification(int orderId, int itemId, string customerId)
            : base(x => x.OrderId == orderId && x.OrderItemId == itemId && x.CustomerId == customerId)
        {
        }
    }
}
