using System;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class CustomerOrdersSpecification : BaseSpecifications<CustomerOrder>
    {
        public CustomerOrdersSpecification(string customerId, CustomerOrderFilterParams filterParams, bool isCountOnly = false)
            : base(o => o.CustomerId == customerId &&
                       (!filterParams.Status.HasValue || o.Status == filterParams.Status.Value) &&
                       (string.IsNullOrEmpty(filterParams.Search) ||
                        (o.TrackingNumber != null && o.TrackingNumber.Contains(filterParams.Search)) ||
                        o.Id.ToString().Contains(filterParams.Search)))
        {
            if (!isCountOnly)
            {
                AddInclude("Items.Product.Images");
                AddOrderByDescending(o => o.CreatedAt);
                ApplyPagination(filterParams.PageIndex, filterParams.PageSize);
            }
        }
    }
}
