using Talentree.Core.Entities;
namespace Talentree.Core.Specifications.Supplier
{
    /// <summary>Used to check for duplicate supplier emails before creating.</summary>
    public class SupplierByEmailSpec : BaseSpecifications<Entities.Supplier>
    {
        public SupplierByEmailSpec(string email)
            : base(s => s.Email == email) { }
    }

    /// <summary>Fetches a single active supplier by ID — used when validating material creation/update.</summary>
    public class SupplierByIdActiveSpec : BaseSpecifications<Entities.Supplier>
    {
        public SupplierByIdActiveSpec(int id)
            : base(s => s.Id == id && s.IsActive) { }
    }
}