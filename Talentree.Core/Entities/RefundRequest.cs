using System;

namespace Talentree.Core.Entities
{
    public class RefundRequest : AuditableEntity
    {
        public int OrderId { get; set; }
        public CustomerOrder Order { get; set; } = null!;
        
        public int OrderItemId { get; set; }
        public CustomerOrderItem OrderItem { get; set; } = null!;
        
        public string CustomerId { get; set; } = null!;
        public string BusinessOwnerId { get; set; } = null!; // Stored for fast BO queries
        
        public string Reason { get; set; } = null!;
        public decimal RefundAmount { get; set; }            // Extracted from OrderItem.LineTotal
        
        public Enums.RefundStatus Status { get; set; } = Enums.RefundStatus.PendingBoResponse;
        public string? BoResponse { get; set; }             // "Accepted" or "Rejected"
        public string? BoNotes { get; set; }
        public string? AdminNotes { get; set; }
        public string? ProcessedById { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? StripeRefundId { get; set; }
    }
}
