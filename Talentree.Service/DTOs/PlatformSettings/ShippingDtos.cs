namespace Talentree.Service.DTOs.PlatformSettings
{
    // ═══════════════════════════════════════════════════════════
    // FR-AD-33: Shipping Configuration DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>Read model for current shipping settings.</summary>
    public class ShippingSettingsDto
    {
        public bool IsFlatRatePerItem { get; set; }
        public decimal FlatRate { get; set; }
        public bool FreeShippingEnabled { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public int EstimatedDeliveryDomesticDays { get; set; }
        public int EstimatedDeliveryInternationalDays { get; set; }
        public bool InternationalShippingEnabled { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>Write model for updating shipping settings.</summary>
    public class UpdateShippingSettingsDto
    {
        /// <summary>True = flat rate per item; False = flat rate per order.</summary>
        public bool IsFlatRatePerItem { get; set; }
        public decimal FlatRate { get; set; }
        public bool FreeShippingEnabled { get; set; }
        public decimal FreeShippingThreshold { get; set; }
        public int EstimatedDeliveryDomesticDays { get; set; }
        public int EstimatedDeliveryInternationalDays { get; set; }
        public bool InternationalShippingEnabled { get; set; }
    }
}
