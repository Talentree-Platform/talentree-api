using System;
using System.Collections.Generic;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.DTOs.Admin.Orders
{
    public class AdminOrderDetailDto : AdminOrderSummaryDto
    {
        public CheckoutDeliveryDto Delivery { get; set; } = null!;
        public decimal SubtotalAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? AdminNotes { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
    }
}
