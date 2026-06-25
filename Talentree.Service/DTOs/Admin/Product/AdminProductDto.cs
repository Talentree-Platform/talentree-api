using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Product row for the FR-AD-10 moderation table (all-products view).
    /// Includes status, visibility, analytics summary, and seller info.
    /// </summary>
    public class AdminProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Tags { get; set; }

        // Category
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Business Owner / Seller
        public int BusinessOwnerProfileId { get; set; }
        public string BusinessOwnerName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;

        // Status & Visibility
        public ProductStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsDeleted { get; set; }
        public string? RejectionReason { get; set; }

        // Images
        public string? MainImageUrl { get; set; }
        public int ImageCount { get; set; }

        // Analytics (summary)
        public long ViewCount { get; set; }
        public long CartAddCount { get; set; }
        public long PurchaseCount { get; set; }
        public decimal RevenueTotal { get; set; }

        /// <summary>PurchaseCount / max(ViewCount,1) * 100 — computed at mapping.</summary>
        public double ConversionRate { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }
}
