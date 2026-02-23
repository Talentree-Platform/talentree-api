using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// Represents a product in the Talentree e-commerce catalog
    /// </summary>
    /// <remarks>
    /// This is a core domain entity that represents physical or digital products
    /// that can be sold on the platform. Each product has basic information
    /// like name, description, price, and image.
    /// </remarks>
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Tags { get; set; } // comma-separated

        // Status workflow
        public ProductStatus Status { get; set; } = ProductStatus.PendingApproval;
        public string? RejectionReason { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // FK - Business Owner
        public int BusinessOwnerProfileId { get; set; }
        public BusinessOwnerProfile BusinessOwner { get; set; } = null!;

        // FK - Category
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Navigation
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}