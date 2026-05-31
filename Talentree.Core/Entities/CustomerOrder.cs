using System;
using System.Collections.Generic;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    public class CustomerOrder : AuditableEntity
    {
        public string CustomerId { get; set; } = null!;
        public AppUser Customer { get; set; } = null!;

        // Delivery snapshot (captured at checkout)
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;

        // Financials
        public decimal SubtotalAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Status
        public CustomerOrderStatus Status { get; set; } = CustomerOrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public PaymentMethod PaymentMethod { get; set; }

        // Stripe (null for Cash on Delivery)
        public string? StripePaymentIntentId { get; set; }

        // Shipping
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        // Admin notes
        public string? AdminNotes { get; set; }

        // Navigation
        public ICollection<CustomerOrderItem> Items { get; set; } = new List<CustomerOrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }
}
