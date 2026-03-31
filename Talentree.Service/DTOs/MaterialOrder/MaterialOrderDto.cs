using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.MaterialOrder
{
    /// <summary>
    /// Submitted by the BO when checking out their material basket.
    /// PaymentMethod removed — Stripe is the only payment method in MVP.
    /// After checkout, call POST /api/payment/material-orders/{id}/create-intent
    /// to initiate the Stripe payment flow.
    /// </summary>
    public class MaterialCheckoutDto
    {
        [Required, MaxLength(500)]
        public string DeliveryAddress { get; set; } = null!;

        [Required, MaxLength(100)]
        public string DeliveryCity { get; set; } = null!;

        [Required, MaxLength(100)]
        public string DeliveryCountry { get; set; } = null!;

        [Required, MaxLength(30)]
        public string ContactPhone { get; set; } = null!;
    }

    /// <summary>
    /// Summary row in the BO's material order history list.
    /// </summary>
    public class MaterialOrderSummaryDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public MaterialOrderStatus Status { get; set; }

        /// <summary>Payment state — Unpaid until Stripe confirms payment.</summary>
        public PaymentStatus PaymentStatus { get; set; }

        public int ItemCount { get; set; }
        public string DeliveryLocation { get; set; } = null!;
    }

    /// <summary>
    /// Full detail of a material order including all line items.
    /// </summary>
    public class MaterialOrderDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public MaterialOrderStatus Status { get; set; }

        /// <summary>Payment state — Unpaid until Stripe confirms payment.</summary>
        public PaymentStatus PaymentStatus { get; set; }

        public string DeliveryAddress { get; set; } = null!;
        public string DeliveryCity { get; set; } = null!;
        public string DeliveryCountry { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public List<MaterialOrderItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// A single line item inside a material order.
    /// </summary>
    public class MaterialOrderItemDto
    {
        public int Id { get; set; }
        public int RawMaterialId { get; set; }
        public string MaterialName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPriceAtPurchase { get; set; }
        public decimal LineTotal { get; set; }
    }
}