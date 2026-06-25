namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Product row in the FR-AD-11 low-stock alerts list.
    /// </summary>
    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        // Seller info
        public int BusinessOwnerProfileId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string SellerUserId { get; set; } = string.Empty;

        // Category
        public string CategoryName { get; set; } = string.Empty;

        // Stock info
        public int CurrentStock { get; set; }
        public bool LowStockFlag { get; set; }

        // The most-recent order date involving this product (approximated via PurchaseCount > 0 + UpdatedAt)
        public DateTime? LastUpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
