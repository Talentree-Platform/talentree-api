using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.Supplier
{
    /// <summary>
    /// Specification for admin supplier listing.
    /// Supports filtering by name/contact person search and active status.
    /// </summary>
    public class SupplierSpec : BaseSpecifications<Entities.Supplier>
    {
        public SupplierSpec(string? search, bool? isActive, int pageIndex, int pageSize)
            : base(s =>
                (string.IsNullOrWhiteSpace(search) || s.Name.Contains(search) || s.ContactPerson.Contains(search)) &&
                (!isActive.HasValue || s.IsActive == isActive.Value))
        {
            AddOrderBy(s => s.Name);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Count specification for supplier listing — used for pagination total.
    /// </summary>
    public class SupplierCountSpec : BaseSpecifications<Entities.Supplier>
    {
        public SupplierCountSpec(string? search, bool? isActive)
            : base(s =>
                (string.IsNullOrWhiteSpace(search) || s.Name.Contains(search) || s.ContactPerson.Contains(search)) &&
                (!isActive.HasValue || s.IsActive == isActive.Value))
        {
        }
    }

    /// <summary>
    /// Specification for fetching a single supplier by ID including their raw materials.
    /// </summary>
    public class SupplierByIdWithMaterialsSpec : BaseSpecifications<Entities.Supplier>
    {
        public SupplierByIdWithMaterialsSpec(int id)
            : base(s => s.Id == id)
        {
            AddInclude(s => s.RawMaterials);
        }
    }
}