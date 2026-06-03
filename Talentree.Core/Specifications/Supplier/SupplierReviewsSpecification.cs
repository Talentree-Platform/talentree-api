using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.Supplier
{
    public class SupplierReviewsSpecification : BaseSpecifications<SupplierReview>
    {
        public SupplierReviewsSpecification(int supplierId, int pageIndex, int pageSize)
            : base(r => r.SupplierId == supplierId && !r.IsDeleted)
        {
            ApplyPagination(pageIndex, pageSize);
            AddInclude(r => r.BusinessOwner); // to get the business owner name
            AddOrderByDescending(r => r.CreatedAt);
        }

        public SupplierReviewsSpecification(int supplierId)
            : base(r => r.SupplierId == supplierId && !r.IsDeleted)
        {
        }
    }
}
