using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// A single entry in the financial ledger for a Business Owner.
    /// Append-only — records are never updated or deleted after creation.
    /// Written automatically by PaymentService when Stripe confirms a payment.
    /// The AI team writes AnomalyFlag and AnomalyScore via their ML pipeline.
    /// </summary>
    public class Transaction : AuditableEntity
    {
        /// <summary>Identity ID of the Business Owner this transaction belongs to.</summary>
        public string BusinessOwnerId { get; set; } = null!;

        /// <summary>
        /// Type of financial movement.
        /// Determines how Amount affects the BO's running balance.
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Human-readable description of the transaction.
        /// Example: "Raw material order #12 — Cotton Fabric x20"
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// Monetary amount in EGP.
        /// Positive = credit (income). Negative = debit (expense).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Running balance after this transaction was applied.
        /// Calculated at write time and stored for fast ledger display.
        /// </summary>
        public decimal BalanceAfter { get; set; }

        /// <summary>
        /// Primary key of the source entity that generated this transaction.
        /// Null for manual entries (fees, adjustments).
        /// </summary>
        public int? ReferenceId { get; set; }

        /// <summary>
        /// Entity type of the source — matches the TransactionType context.
        /// Values: "MaterialOrder", "ProductionRequest", "PayoutRequest"
        /// </summary>
        public string? ReferenceType { get; set; }

        /// <summary>
        /// Stripe PaymentIntent ID for traceability.
        /// Allows correlating the ledger entry back to the Stripe event.
        /// </summary>
        public string? StripePaymentIntentId { get; set; }

        // AI/Analytics fields

        /// <summary>
        /// AI: Set to true when the anomaly detection model flags this transaction.
        /// Backend always inserts as false — AI pipeline updates asynchronously.
        /// </summary>
        public bool AnomalyFlag { get; set; } = false;

        /// <summary>
        /// AI: Anomaly score output from Isolation Forest / LSTM model (0.0–1.0).
        /// Null until the AI pipeline processes this transaction.
        /// </summary>
        public float? AnomalyScore { get; set; }
    }
}