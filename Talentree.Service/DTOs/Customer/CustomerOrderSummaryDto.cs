using System;

namespace Talentree.Service.DTOs.Customer
{
    public class CustomerOrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public int ItemCount { get; set; }
    }
}
