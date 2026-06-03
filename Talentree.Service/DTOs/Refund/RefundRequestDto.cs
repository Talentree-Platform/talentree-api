using System;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.DTOs.Refund
{
    public class RefundRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public OrderItemDto OrderItem { get; set; } = null!;
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string BusinessOwnerId { get; set; } = null!;
        public string BusinessOwnerName { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public decimal RefundAmount { get; set; }
        public string Status { get; set; } = null!;
        public string? BoResponse { get; set; }
        public string? BoNotes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
