using System;
using System.Linq;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.Supplier
{
    public class SupplierPerformanceSpecification : BaseSpecifications<Entities.Supplier>
    {
        public SupplierPerformanceSpecification(int pageIndex, int pageSize, string? sortBy)
            : base(s => !s.IsDeleted)
        {
            ApplyPagination(pageIndex, pageSize);

            // We need to load everything related to performance
            AddInclude(s => s.SupplierReviews);
            AddInclude(s => s.SupportTickets);
            AddInclude("RawMaterials.MaterialOrderItems.Order");

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        AddOrderBy(s => s.Name);
                        break;
                    case "namedesc":
                        AddOrderByDescending(s => s.Name);
                        break;
                    // Order-based sorting (total orders, revenue, rating, issue rate) 
                    // is very difficult to do in EF Core via generic OrderBy on navigation collections,
                    // so we will default to name, and if we need advanced sorting, we'll handle it in the service layer or map it to a queryable DTO projection.
                    default:
                        AddOrderBy(s => s.Name);
                        break;
                }
            }
            else
            {
                AddOrderBy(s => s.Name);
            }
        }

        // Constructor for counting
        public SupplierPerformanceSpecification() : base(s => !s.IsDeleted)
        {
        }
    }
}
