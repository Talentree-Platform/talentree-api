using System;
using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class CustomerOrderDetailDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public CheckoutDeliveryDto Delivery { get; set; } = null!;
        public decimal SubtotalAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
    }
}
