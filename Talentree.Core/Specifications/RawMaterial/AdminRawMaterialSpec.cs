using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.RawMaterial
{
    /// <summary>
    /// Specification for admin raw material listing.
    /// Admin sees ALL materials including unavailable ones.
    /// Supports filtering by category, search, and availability.
    /// </summary>
    public class AdminRawMaterialSpec : BaseSpecifications<Entities.RawMaterial>
    {
        public AdminRawMaterialSpec(
            string? category,
            string? search,
            bool? isAvailable,
            int pageIndex,
            int pageSize)
            : base(m =>
                (string.IsNullOrWhiteSpace(category) || m.Category.Contains(category)) &&
                (string.IsNullOrWhiteSpace(search) || m.Name.Contains(search) || m.Description.Contains(search)) &&
                (!isAvailable.HasValue || m.IsAvailable == isAvailable.Value))
        {
            AddInclude(m => m.Supplier);
            AddOrderBy(m => m.Category);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Count specification for admin raw material listing — used for pagination total.
    /// </summary>
    public class AdminRawMaterialCountSpec : BaseSpecifications<Entities.RawMaterial>
    {
        public AdminRawMaterialCountSpec(string? category, string? search, bool? isAvailable)
            : base(m =>
                (string.IsNullOrWhiteSpace(category) || m.Category.Contains(category)) &&
                (string.IsNullOrWhiteSpace(search) || m.Name.Contains(search) || m.Description.Contains(search)) &&
                (!isAvailable.HasValue || m.IsAvailable == isAvailable.Value))
        {
        }
    }

    /// <summary>
    /// Specification for fetching a single raw material by ID with supplier (admin — no IsAvailable filter).
    /// </summary>
    public class AdminRawMaterialByIdSpec : BaseSpecifications<Entities.RawMaterial>
    {
        public AdminRawMaterialByIdSpec(int id)
            : base(m => m.Id == id)
        {
            AddInclude(m => m.Supplier);
        }
    }
}