namespace Talentree.Service.DTOs.PlatformSettings
{
    // ═══════════════════════════════════════════════════════════
    // FR-AD-32: Commission & Fee Configuration DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>Read model for current commission settings.</summary>
    public class CommissionSettingDto
    {
        public decimal PlatformCommissionPercent { get; set; }
        public bool IsTransactionFeePercent { get; set; }
        public decimal TransactionFeeValue { get; set; }
        public decimal MinimumPayoutAmount { get; set; }
        public decimal PayoutProcessingFee { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    /// <summary>Write model for updating commission settings.</summary>
    public class UpdateCommissionSettingDto
    {
        /// <summary>Percentage of each sale retained by the platform (0–100).</summary>
        public decimal PlatformCommissionPercent { get; set; }

        /// <summary>True = TransactionFeeValue is a %; False = it is a fixed amount.</summary>
        public bool IsTransactionFeePercent { get; set; }

        public decimal TransactionFeeValue { get; set; }
        public decimal MinimumPayoutAmount { get; set; }
        public decimal PayoutProcessingFee { get; set; }
    }
}
