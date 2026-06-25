namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-32: Platform commission and fee configuration.
    /// Single-row settings table — always use Id = 1 (upsert pattern).
    /// Changes here apply to new transactions only; existing Transaction records are unchanged.
    /// </summary>
    public class CommissionSetting : AuditableEntity
    {
        // ── Commission ─────────────────────────────────────────────
        /// <summary>Percentage of each sale retained by the platform (0–100).</summary>
        public decimal PlatformCommissionPercent { get; set; }

        // ── Transaction Fee ─────────────────────────────────────────
        /// <summary>When true, TransactionFeeValue is a percentage; when false it is a fixed amount.</summary>
        public bool IsTransactionFeePercent { get; set; }

        /// <summary>Transaction fee amount (% or fixed depending on IsTransactionFeePercent).</summary>
        public decimal TransactionFeeValue { get; set; }

        // ── Payout ──────────────────────────────────────────────────
        /// <summary>Minimum payout amount that can be requested by a seller.</summary>
        public decimal MinimumPayoutAmount { get; set; }

        /// <summary>Fixed fee deducted when processing a payout.</summary>
        public decimal PayoutProcessingFee { get; set; }
    }
}
