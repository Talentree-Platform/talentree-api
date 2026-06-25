using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    /// <summary>
    /// FR-AD-10: All-products specification for the admin moderation table.
    /// Supports filtering by status, category, seller, price range, date range, and text search.
    /// Includes BusinessOwner (with User), Category, and Images.
    /// Bypasses soft-delete global filter so admins can see soft-deleted products.
    /// </summary>
    public class AdminProductsSpecification : BaseSpecifications<Product>
    {
        /// <summary>Count-only constructor (no pagination).</summary>
        public AdminProductsSpecification(
            ProductStatus? status = null,
            int? categoryId = null,
            int? businessOwnerProfileId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? search = null,
            string? sortBy = "createdAt",
            bool sortDesc = true)
            : base(BuildCriteria(status, categoryId, businessOwnerProfileId, minPrice, maxPrice, fromDate, toDate, search))
        {
            AddIncludes();
            ApplySorting(sortBy, sortDesc);
            EnableIgnoreQueryFilters();
        }

        /// <summary>Paginated constructor.</summary>
        public AdminProductsSpecification(
            int pageIndex,
            int pageSize,
            ProductStatus? status = null,
            int? categoryId = null,
            int? businessOwnerProfileId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? search = null,
            string? sortBy = "createdAt",
            bool sortDesc = true)
            : base(BuildCriteria(status, categoryId, businessOwnerProfileId, minPrice, maxPrice, fromDate, toDate, search))
        {
            AddIncludes();
            ApplySorting(sortBy, sortDesc);
            ApplyPagination(pageIndex, pageSize);
            EnableIgnoreQueryFilters();
        }

        private void AddIncludes()
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Images);
            AddInclude("BusinessOwner.User");
        }

        private void ApplySorting(string? sortBy, bool sortDesc)
        {
            switch (sortBy?.ToLower())
            {
                case "name":
                    if (sortDesc) AddOrderByDescending(p => p.Name);
                    else AddOrderBy(p => p.Name);
                    break;
                case "price":
                    if (sortDesc) AddOrderByDescending(p => p.Price);
                    else AddOrderBy(p => p.Price);
                    break;
                case "views":
                    if (sortDesc) AddOrderByDescending(p => p.ViewCount);
                    else AddOrderBy(p => p.ViewCount);
                    break;
                case "stock":
                    if (sortDesc) AddOrderByDescending(p => p.StockQuantity);
                    else AddOrderBy(p => p.StockQuantity);
                    break;
                default: // "createdAt"
                    if (sortDesc) AddOrderByDescending(p => p.CreatedAt);
                    else AddOrderBy(p => p.CreatedAt);
                    break;
            }
        }

        private static System.Linq.Expressions.Expression<Func<Product, bool>> BuildCriteria(
            ProductStatus? status,
            int? categoryId,
            int? businessOwnerProfileId,
            decimal? minPrice,
            decimal? maxPrice,
            DateTime? fromDate,
            DateTime? toDate,
            string? search)
        {
            return p =>
                (status == null || p.Status == status) &&
                (categoryId == null || p.CategoryId == categoryId) &&
                (businessOwnerProfileId == null || p.BusinessOwnerProfileId == businessOwnerProfileId) &&
                (minPrice == null || p.Price >= minPrice) &&
                (maxPrice == null || p.Price <= maxPrice) &&
                (fromDate == null || p.CreatedAt >= fromDate) &&
                (toDate == null || p.CreatedAt <= toDate) &&
                (search == null || p.Name.ToLower().Contains(search.ToLower()));
        }
    }
}
