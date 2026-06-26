namespace Talentree.Service.DTOs.PlatformSettings
{
    // ═══════════════════════════════════════════════════════════
    // FR-AD-34: Tax Configuration DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>Read model for current tax settings.</summary>
    public class TaxSettingsDto
    {
        public bool TaxEnabled { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsInclusive { get; set; }
        public List<int> TaxExemptCategoryIds { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>Write model for updating tax settings.</summary>
    public class UpdateTaxSettingsDto
    {
        public bool TaxEnabled { get; set; }

        /// <summary>Tax rate as a percentage (0–100).</summary>
        public decimal TaxRate { get; set; }

        /// <summary>True = price already includes tax; False = tax added on top.</summary>
        public bool IsInclusive { get; set; }

        /// <summary>Category IDs that are exempt from tax. Empty list = all taxable.</summary>
        public List<int> TaxExemptCategoryIds { get; set; } = new();
    }
}
