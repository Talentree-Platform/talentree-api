using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// A confirmed purchase order created when a Business Owner checks out their basket.
    /// Prices are locked at checkout time — changes to the raw-material catalogue do not affect existing orders.
    /// </summary>
    public class MaterialOrder : AuditableEntity
    {
        /// <summary>Identity ID of the Business Owner who placed this order.</summary>
        public string BusinessOwnerId { get; set; } = null!;

        // ── Delivery snapshot ─────────────────────────────────────
        /// <summary>Full street / building delivery address captured at checkout.</summary>
        public string DeliveryAddress { get; set; } = null!;

        /// <summary>Delivery city captured at checkout.</summary>
        public string DeliveryCity { get; set; } = null!;

        /// <summary>Delivery country captured at checkout.</summary>
        public string DeliveryCountry { get; set; } = null!;

        /// <summary>Contact phone number captured at checkout for delivery coordination.</summary>
        public string ContactPhone { get; set; } = null!;

        // ── Payment ───────────────────────────────────────────────

        /// <summary>
        /// Stripe PaymentIntent ID — set when BO calls create-intent.
        /// Null until payment is initiated.
        /// </summary>
        public string? StripePaymentIntentId { get; set; }

        /// <summary>
        /// Payment lifecycle state driven by Stripe webhook events.
        /// Never set manually.
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        // ── Amounts ───────────────────────────────────────────────
        /// <summary>
        /// Total order value calculated at checkout as the sum of all line totals.
        /// Never recalculated after the order is placed.
        /// </summary>
        public decimal TotalAmount { get; set; }

        // ── Status ────────────────────────────────────────────────
        /// <summary>Current fulfilment status of the order.</summary>
        public MaterialOrderStatus Status { get; set; } = MaterialOrderStatus.Pending;

        // ── Navigation ────────────────────────────────────────────
        /// <summary>The individual raw-material lines that make up this order.</summary>
        public ICollection<MaterialOrderItem> Items { get; set; } = new List<MaterialOrderItem>();
    }
}