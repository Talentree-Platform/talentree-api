// Talentree.Core/Entities/RawMaterial.cs

namespace Talentree.Core.Entities
{
    /// <summary>
    /// Raw materials that suppliers provide to business owners
    /// </summary>
    public class RawMaterial : AuditableEntity, ISoftDelete
    {
        /// <summary>
        /// Name of raw material
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Price per unit
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Unit of measurement (kg, liter, meter, piece, etc.)
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Minimum order quantity
        /// </summary>
        public int MinimumOrderQuantity { get; set; } = 1;

        /// <summary>
        /// Current stock availability
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Is this material currently available
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Category of raw material (Fabric, Metal, Wood, etc.)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Image URL for the raw material
        /// </summary>
        public string? PictureUrl { get; set; }

        /// <summary>
        /// Foreign key to Supplier
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Navigation property to Supplier
        /// </summary>
        public Supplier Supplier { get; set; } = null!;

        // From ISoftDelete:
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }


    }
}