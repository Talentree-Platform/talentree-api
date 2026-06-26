namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-33: Shipping configuration for the platform.
    /// Single-row settings table — always use Id = 1 (upsert pattern).
    /// </summary>
    public class ShippingSettings : AuditableEntity
    {
        // ── Flat Rate ───────────────────────────────────────────────
        /// <summary>When true, flat rate is applied per item; when false, per order.</summary>
        public bool IsFlatRatePerItem { get; set; }

        /// <summary>Flat shipping rate value (applied per item or per order).</summary>
        public decimal FlatRate { get; set; }

        // ── Free Shipping ───────────────────────────────────────────
        /// <summary>When true, free shipping is offered above FreeShippingThreshold.</summary>
        public bool FreeShippingEnabled { get; set; }

        /// <summary>Order subtotal above which free shipping is applied.</summary>
        public decimal FreeShippingThreshold { get; set; }

        // ── Delivery Estimates ──────────────────────────────────────
        /// <summary>Estimated domestic delivery time in business days.</summary>
        public int EstimatedDeliveryDomesticDays { get; set; }

        /// <summary>Estimated international delivery time in business days.</summary>
        public int EstimatedDeliveryInternationalDays { get; set; }

        /// <summary>When false, international shipping is not offered.</summary>
        public bool InternationalShippingEnabled { get; set; }
    }
}
