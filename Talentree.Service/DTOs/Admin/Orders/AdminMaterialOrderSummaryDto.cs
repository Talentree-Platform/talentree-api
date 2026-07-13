using System;

namespace Talentree.Service.DTOs.Admin.Orders
{
    public class AdminMaterialOrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BusinessOwnerId { get; set; } = null!;
        public string BusinessOwnerName { get; set; } = null!;
        public string BusinessOwnerEmail { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public int ItemCount { get; set; }
        public string DeliveryLocation { get; set; } = null!;
    }
}
