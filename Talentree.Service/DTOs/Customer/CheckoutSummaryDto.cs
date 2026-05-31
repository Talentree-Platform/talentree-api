using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class CheckoutSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal Total { get; set; }
        public CheckoutDeliveryDto Delivery { get; set; } = null!;
    }
}
