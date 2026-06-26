namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-34: Tax configuration for the platform (government compliance).
    /// Single-row settings table — always use Id = 1 (upsert pattern).
    /// </summary>
    public class TaxSettings : AuditableEntity
    {
        /// <summary>When false, no tax is collected or displayed at checkout.</summary>
        public bool TaxEnabled { get; set; }

        /// <summary>Tax rate as a percentage (0–100).</summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        /// When true, the product price already includes tax (inclusive display).
        /// When false, tax is added on top (exclusive display).
        /// </summary>
        public bool IsInclusive { get; set; }

        /// <summary>
        /// Optional comma-separated Category IDs that are exempt from tax.
        /// Null or empty means all categories are taxable.
        /// </summary>
        public string? TaxExemptCategoryIds { get; set; }
    }
}
