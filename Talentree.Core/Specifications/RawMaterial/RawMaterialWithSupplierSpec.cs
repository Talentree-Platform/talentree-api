using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.RawMaterial
{
    /// <summary>
    /// Specification for browsing available raw materials as a Business Owner.
    /// Filters by BO's business category, optional sub-category, and search term.
    /// Includes supplier navigation property.
    /// </summary>
    public class RawMaterialWithSupplierSpec : BaseSpecifications<Entities.RawMaterial>
    {
        public RawMaterialWithSupplierSpec(
            string businessCategory,
            string? category,
            string? search,
            int pageIndex,
            int pageSize)
            : base(m =>
                m.IsAvailable &&
                (m.Category.Contains(businessCategory) || category != null) &&
                (string.IsNullOrWhiteSpace(category) || m.Category == category) &&
                (string.IsNullOrWhiteSpace(search) || m.Name.Contains(search) || m.Description.Contains(search)))
        {
            AddInclude(m => m.Supplier);
            AddOrderBy(m => m.Name);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Specification for fetching a single available raw material by ID with supplier.
    /// </summary>
    public class RawMaterialByIdWithSupplierSpec : BaseSpecifications<Entities.RawMaterial>
    {
        public RawMaterialByIdWithSupplierSpec(int id)
            : base(m => m.Id == id && m.IsAvailable)
        {
            AddInclude(m => m.Supplier);
        }
    }

    /// <summary>
    /// Count specification for BO raw material browsing — used for pagination total.
    /// Must match the same criteria as RawMaterialWithSupplierSpec without includes or pagination.
    /// </summary>
    public class RawMaterialCountSpec : BaseSpecifications<Entities.RawMaterial>
    {
        public RawMaterialCountSpec(string businessCategory, string? category, string? search)
            : base(m =>
                m.IsAvailable &&
                (m.Category.Contains(businessCategory) || category != null) &&
                (string.IsNullOrWhiteSpace(category) || m.Category == category) &&
                (string.IsNullOrWhiteSpace(search) || m.Name.Contains(search) || m.Description.Contains(search)))
        {
        }
    }
}