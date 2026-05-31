using System;

namespace Talentree.Service.DTOs.Customer
{
    public class WishlistItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductImageUrl { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductStockQuantity { get; set; }
        public string ProductBrandName { get; set; } = null!;
        public DateTime AddedAt { get; set; }
    }
}
