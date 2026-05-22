using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal EstimatedShipping { get; set; }
        public decimal Total { get; set; }
    }
}
