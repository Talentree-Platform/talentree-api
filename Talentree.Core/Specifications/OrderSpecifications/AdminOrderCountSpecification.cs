using System;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class AdminOrderCountSpecification : BaseSpecifications<CustomerOrder>
    {
        public AdminOrderCountSpecification(string? search, CustomerOrderStatus? status, PaymentStatus? paymentStatus, DateTime? dateFrom, DateTime? dateTo)
            : base(x => 
                (string.IsNullOrEmpty(search) || x.Customer.DisplayName.Contains(search) || x.Customer.Email.Contains(search)) &&
                (!status.HasValue || x.Status == status) &&
                (!paymentStatus.HasValue || x.PaymentStatus == paymentStatus) &&
                (!dateFrom.HasValue || x.CreatedAt >= dateFrom) &&
                (!dateTo.HasValue || x.CreatedAt <= dateTo)
            )
        {
        }
    }
}
