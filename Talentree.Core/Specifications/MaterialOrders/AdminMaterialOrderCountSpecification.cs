using System;
using System.Collections.Generic;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.MaterialOrders
{
    public class AdminMaterialOrderCountSpecification : BaseSpecifications<MaterialOrder>
    {
        public AdminMaterialOrderCountSpecification(
            List<string>? matchingBoIds,
            MaterialOrderStatus? status,
            PaymentStatus? paymentStatus,
            DateTime? dateFrom,
            DateTime? dateTo)
            : base(x =>
                (matchingBoIds == null || matchingBoIds.Contains(x.BusinessOwnerId)) &&
                (!status.HasValue || x.Status == status) &&
                (!paymentStatus.HasValue || x.PaymentStatus == paymentStatus) &&
                (!dateFrom.HasValue || x.CreatedAt >= dateFrom) &&
                (!dateTo.HasValue || x.CreatedAt <= dateTo)
            )
        {
        }
    }
}
