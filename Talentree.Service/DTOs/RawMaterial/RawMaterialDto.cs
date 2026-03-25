
namespace Talentree.Service.DTOs.RawMaterial
{
    /// <summary>
    /// Used in both the raw material store browsing
    /// AND in the production order material selection dropdown
    /// </summary>
    public class RawMaterialDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string Unit { get; set; } = null!;
        public int MinimumOrderQuantity { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string Category { get; set; } = null!;
        public string? PictureUrl { get; set; }
        public string SupplierName { get; set; } = null!;
    }
}
