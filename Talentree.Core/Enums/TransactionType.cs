namespace Talentree.Core.Enums
{
    /// <summary>
    /// Classifies a ledger entry in the Transactions table.
    /// Determines sign convention: Sale/Payout = credit. Everything else = debit.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>Revenue from a customer purchasing a BO's product (Phase 2).</summary>
        Sale = 0,

        /// <summary>Expense from a BO purchasing raw materials from Talentree's store.</summary>
        MaterialPurchase = 1,

        /// <summary>Expense from a BO paying for a production service request.</summary>
        ProductionRequest = 2,

        /// <summary>Reversal of a previous transaction.</summary>
        Refund = 3,

        /// <summary>Funds disbursed to the BO's bank account.</summary>
        Payout = 4,

        /// <summary>Platform fee deducted from BO's balance.</summary>
        Fee = 5
    }
}