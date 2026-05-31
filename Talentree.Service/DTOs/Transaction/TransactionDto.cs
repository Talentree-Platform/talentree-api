using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Transaction
{
    /// <summary>
    /// A single row in the BO's transaction history ledger.
    /// </summary>
    public class TransactionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = null!;

        /// <summary>Positive = credit. Negative = debit.</summary>
        public decimal Amount { get; set; }

        /// <summary>Running balance after this transaction.</summary>
        public decimal BalanceAfter { get; set; }

        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
    }

    /// <summary>
    /// Financial summary for the dashboard cards.
    /// Calculated from the Transactions ledger for a given date range.
    /// </summary>
    public class FinancialSummaryDto
    {
        /// <summary>Total revenue (sum of Sale transactions) in period.</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Total expenses (MaterialPurchase + ProductionRequest) in period.</summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>Revenue minus Expenses.</summary>
        public decimal NetProfit => TotalRevenue - TotalExpenses;

        /// <summary>Current available balance (all-time credits minus debits).</summary>
        public decimal AvailableBalance { get; set; }

        /// <summary>Sum of pending payout requests.</summary>
        public decimal PendingPayouts { get; set; }

        /// <summary>Start of the reporting period.</summary>
        public DateTime PeriodFrom { get; set; }

        /// <summary>End of the reporting period.</summary>
        public DateTime PeriodTo { get; set; }
    }
}