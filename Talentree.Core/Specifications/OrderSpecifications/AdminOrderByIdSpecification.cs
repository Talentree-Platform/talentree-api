using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class AdminOrderByIdSpecification : BaseSpecifications<CustomerOrder>
    {
        public AdminOrderByIdSpecification(int id) : base(x => x.Id == id)
        {
            AddInclude(x => x.Customer);
            AddInclude(x => x.Items);
            AddInclude("Items.Product");
            AddInclude("Items.Product.BusinessOwner");
            AddInclude(x => x.StatusHistory);
            
            AddOrderByDescending(x => x.CreatedAt); // Unnecessary for one item, but harmless
        }
    }
}
