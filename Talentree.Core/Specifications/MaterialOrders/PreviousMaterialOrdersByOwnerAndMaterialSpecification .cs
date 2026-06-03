using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.MaterialOrders
{
    public class PreviousMaterialOrdersByOwnerAndMaterialSpecification
      : BaseSpecifications<MaterialOrder>
    {
        public PreviousMaterialOrdersByOwnerAndMaterialSpecification(
            string businessOwnerId,
            int rawMaterialId)
            : base(mo =>
                mo.BusinessOwnerId == businessOwnerId &&
                mo.Items.Any(i => i.RawMaterialId == rawMaterialId) &&
                mo.Status != MaterialOrderStatus.Cancelled)
        {
            AddOrderByDescending(mo => mo.CreatedAt);
        }
    }
}
