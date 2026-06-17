using System;

namespace Talentree.Repository.Data.DataSeed
{
    public class RawMaterialSeedDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int MinimumOrderQuantity { get; set; }
        public int StockQuantity { get; set; }
        public bool? IsAvailable { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public int SupplierId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public int? OrderFrequency { get; set; }
        public string? PriceTrend { get; set; }
    }
}
