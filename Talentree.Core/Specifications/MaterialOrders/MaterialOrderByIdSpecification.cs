using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.MaterialOrders
{
    public class MaterialOrderByIdSpecification : BaseSpecifications<MaterialOrder>
    {
        public MaterialOrderByIdSpecification(int id)
            : base(o => o.Id == id)
        {
            AddInclude(o => o.Items);
            AddInclude("Items.RawMaterial");
        }
    }
}
