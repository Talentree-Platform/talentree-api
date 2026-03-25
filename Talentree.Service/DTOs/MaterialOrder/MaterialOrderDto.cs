using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.MaterialOrder
{
    // ──────────────────────────────────────────────────────────
    // Input DTOs
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Submitted by the BO when checking out their basket to place a material order.
    /// All basket items become locked order lines at the point of checkout.
    /// </summary>
    public class MaterialCheckoutDto
    {
        /// <summary>Full street / building delivery address.</summary>
        [Required, MaxLength(500)]
        public string DeliveryAddress { get; set; } = null!;

        /// <summary>Delivery city.</summary>
        [Required, MaxLength(100)]
        public string DeliveryCity { get; set; } = null!;

        /// <summary>Delivery country.</summary>
        [Required, MaxLength(100)]
        public string DeliveryCountry { get; set; } = null!;

        /// <summary>Contact phone number for delivery coordination.</summary>
        [Required, MaxLength(30)]
        public string ContactPhone { get; set; } = null!;

        /// <summary>
        /// Payment method for this order.
        /// Use "CashOnDelivery" until Stripe is integrated.
        /// </summary>
        [Required, MaxLength(100)]
        public string PaymentMethod { get; set; } = null!;
    }

    // ──────────────────────────────────────────────────────────
    // Response DTOs
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// A summary row returned in the BO's material order history list.
    /// Contains enough information to render the list without loading full line items.
    /// </summary>
    public class MaterialOrderSummaryDto
    {
        /// <summary>Order primary key.</summary>
        public int Id { get; set; }

        /// <summary>Timestamp when the order was placed.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Total amount charged — sum of all line totals, locked at checkout.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Current fulfilment status of the order.</summary>
        public MaterialOrderStatus Status { get; set; }

        /// <summary>Number of distinct raw-material lines in the order.</summary>
        public int ItemCount { get; set; }

        /// <summary>Combined delivery location for list display — e.g. "Cairo, Egypt".</summary>
        public string DeliveryLocation { get; set; } = null!;
    }

    /// <summary>
    /// Full detail of a single material order including all line items.
    /// Returned immediately after checkout and from the order detail endpoint.
    /// </summary>
    public class MaterialOrderDto
    {
        /// <summary>Order primary key.</summary>
        public int Id { get; set; }

        /// <summary>Timestamp when the order was placed.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Current fulfilment status.</summary>
        public MaterialOrderStatus Status { get; set; }

        /// <summary>Delivery address captured at checkout.</summary>
        public string DeliveryAddress { get; set; } = null!;

        /// <summary>Delivery city captured at checkout.</summary>
        public string DeliveryCity { get; set; } = null!;

        /// <summary>Delivery country captured at checkout.</summary>
        public string DeliveryCountry { get; set; } = null!;

        /// <summary>Contact phone captured at checkout.</summary>
        public string ContactPhone { get; set; } = null!;

        /// <summary>Payment method used at checkout.</summary>
        public string PaymentMethod { get; set; } = null!;

        /// <summary>Total order value — sum of all line totals, never recalculated.</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Individual order line items.</summary>
        public List<MaterialOrderItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// A single line item inside a material order.
    /// Unit price is locked at checkout — never affected by later catalogue changes.
    /// </summary>
    public class MaterialOrderItemDto
    {
        /// <summary>Line item primary key.</summary>
        public int Id { get; set; }

        /// <summary>Raw material primary key.</summary>
        public int RawMaterialId { get; set; }

        /// <summary>Name of the raw material.</summary>
        public string MaterialName { get; set; } = null!;

        /// <summary>Unit of measurement.</summary>
        public string Unit { get; set; } = null!;

        /// <summary>Quantity ordered.</summary>
        public int Quantity { get; set; }

        /// <summary>Unit price locked at the moment of checkout.</summary>
        public decimal UnitPriceAtPurchase { get; set; }

        /// <summary>Quantity × UnitPriceAtPurchase.</summary>
        public decimal LineTotal { get; set; }
    }
}