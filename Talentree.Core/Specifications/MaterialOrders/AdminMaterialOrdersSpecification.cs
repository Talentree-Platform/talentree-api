using System;
using System.Collections.Generic;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.MaterialOrders
{
    public class AdminMaterialOrdersSpecification : BaseSpecifications<MaterialOrder>
    {
        public AdminMaterialOrdersSpecification(
            List<string>? matchingBoIds,
            MaterialOrderStatus? status,
            PaymentStatus? paymentStatus,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? sortBy,
            bool sortDesc,
            int pageIndex,
            int pageSize)
            : base(x =>
                (matchingBoIds == null || matchingBoIds.Contains(x.BusinessOwnerId)) &&
                (!status.HasValue || x.Status == status) &&
                (!paymentStatus.HasValue || x.PaymentStatus == paymentStatus) &&
                (!dateFrom.HasValue || x.CreatedAt >= dateFrom) &&
                (!dateTo.HasValue || x.CreatedAt <= dateTo)
            )
        {
            AddInclude(x => x.Items);
            ApplyPagination(pageIndex, pageSize);

            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.ToLower() == "amount")
                {
                    if (sortDesc) AddOrderByDescending(x => x.TotalAmount);
                    else AddOrderBy(x => x.TotalAmount);
                }
                else
                {
                    if (sortDesc) AddOrderByDescending(x => x.CreatedAt);
                    else AddOrderBy(x => x.CreatedAt);
                }
            }
            else
            {
                AddOrderByDescending(x => x.CreatedAt);
            }
        }
    }
}
