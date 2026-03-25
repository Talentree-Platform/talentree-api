// Talentree.Core/Entities/Supplier.cs

namespace Talentree.Core.Entities
{
    /// <summary>
    /// Supplier entity - managed by Admin only
    /// No user account - static data in database
    /// </summary>
    public class Supplier : AuditableEntity, ISoftDelete
    {
        /// <summary>
        /// Supplier company/business name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Brief description of supplier and their products
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Contact email for business inquiries
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Primary contact phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Physical address of supplier
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// City where supplier is located
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Country of supplier
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Tax identification number
        /// </summary>
        public string? TaxId { get; set; }

        /// <summary>
        /// Name of primary contact person
        /// </summary>
        public string ContactPerson { get; set; } = string.Empty;

        /// <summary>
        /// Is supplier currently active and available
        /// </summary>
        public bool IsActive { get; set; } = true;


        /// <summary>
        /// Raw materials provided by this supplier
        /// </summary>
        public ICollection<RawMaterial> RawMaterials { get; set; } = new List<RawMaterial>();


        // From ISoftDelete:
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }


    }
}