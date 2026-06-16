using System;

namespace Talentree.Repository.Data.DataSeed
{
    public class ProductSeedDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public int BusinessOwnerProfileId { get; set; }
        public int CategoryId { get; set; }
        public string? RejectionReason { get; set; }
        public int? Status { get; set; }
        public int StockQuantity { get; set; }
        public string? Tags { get; set; }
        public float? AvgRating { get; set; }
        public int? CartAddCount { get; set; }
        public int? DemandForecastQty { get; set; }
        public DateTime? DemandForecastUpdatedAt { get; set; }
        public float? DescriptionQualityScore { get; set; }
        public bool? LowStockFlag { get; set; }
        public int? PurchaseCount { get; set; }
        public decimal? RevenueTotal { get; set; }
        public int? ViewCount { get; set; }
        public string? DeletedBy { get; set; }
        public bool? IsVisible { get; set; }
        public int? FeaturedOrder { get; set; }
        public bool? IsFeatured { get; set; }
    }
}
