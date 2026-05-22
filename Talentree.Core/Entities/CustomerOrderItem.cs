namespace Talentree.Core.Entities
{
    public class CustomerOrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public CustomerOrder Order { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Snapshots at purchase time (prices must never change after order)
        public string ProductName { get; set; } = null!;
        public string? ProductImageUrl { get; set; }
        public string SellerName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
