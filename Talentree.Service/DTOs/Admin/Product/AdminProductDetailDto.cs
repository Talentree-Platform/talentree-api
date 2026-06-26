using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Full product detail for the FR-AD-09 review panel.
    /// Extends the list-row data with all image URLs and AI quality signals.
    /// </summary>
    public class AdminProductDetailDto
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
        public string? RejectionReason { get; set; }

        // Images — full list of URLs
        public List<string> ImageUrls { get; set; } = new();

        // AI / Quality signals
        public float? DescriptionQualityScore { get; set; }
        public bool LowStockFlag { get; set; }

        // Analytics
        public long ViewCount { get; set; }
        public long CartAddCount { get; set; }
        public long PurchaseCount { get; set; }
        public decimal RevenueTotal { get; set; }
        public float? AvgRating { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }
}
