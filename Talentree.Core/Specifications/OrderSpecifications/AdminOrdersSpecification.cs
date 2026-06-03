using System;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class AdminOrdersSpecification : BaseSpecifications<CustomerOrder>
    {
        public AdminOrdersSpecification(string? search, CustomerOrderStatus? status, PaymentStatus? paymentStatus, DateTime? dateFrom, DateTime? dateTo, string? sortBy, bool sortDesc, int pageIndex, int pageSize)
            : base(x => 
                (string.IsNullOrEmpty(search) || x.Customer.DisplayName.Contains(search) || x.Customer.Email.Contains(search)) &&
                (!status.HasValue || x.Status == status) &&
                (!paymentStatus.HasValue || x.PaymentStatus == paymentStatus) &&
                (!dateFrom.HasValue || x.CreatedAt >= dateFrom) &&
                (!dateTo.HasValue || x.CreatedAt <= dateTo)
            )
        {
            AddInclude(x => x.Customer);
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
