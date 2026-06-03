using System;

namespace Talentree.Service.DTOs.Admin.Orders
{
    public class AdminOrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string SellerNames { get; set; } = null!; // Comma-separated
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public int ItemCount { get; set; }
    }
}
