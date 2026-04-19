using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// A manufacturing commission submitted by a Business Owner to Talentree.
    /// The BO describes what they want produced and which raw materials to use.
    /// Talentree reviews the request manually, sends a price quote, and starts
    /// production only after the BO confirms the quote.
    /// </summary>
    public class BoProductionRequest : AuditableEntity
    {
        /// <summary>Identity ID of the Business Owner who submitted this request.</summary>
        public string BusinessOwnerId { get; set; } = null!;

        // ── What the BO wants ─────────────────────────────────────
        /// <summary>
        /// Short descriptive title for the request.
        /// Example: "Summer Collection Batch — Hoodies and Bags"
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Optional free-text notes from the BO:
        /// deadlines, design preferences, special requirements, etc.
        /// </summary>
        public string? Notes { get; set; }

        // ── Status ────────────────────────────────────────────────
        /// <summary>
        /// Current lifecycle status of the request.
        /// Transitions are enforced in the service layer.
        /// </summary>
        public BoProductionRequestStatus Status { get; set; } = BoProductionRequestStatus.Submitted;

        // ── Talentree response — filled by Admin ──────────────────
        /// <summary>
        /// Price quoted by Talentree after reviewing the request.
        /// Null until an Admin sends a quote.
        /// </summary>
        public decimal? QuotedPrice { get; set; }

        /// <summary>
        /// Admin message visible to the BO — quote breakdown, rejection reason, or update notes.
        /// </summary>
        public string? AdminNotes { get; set; }

        // Payment
        /// <summary>
        /// Stripe PaymentIntent ID — set when BO initiates payment for the quoted price.
        /// Null until payment is initiated.
        /// </summary>
        public string? StripePaymentIntentId { get; set; }

        /// <summary>
        /// Payment lifecycle state driven by Stripe webhook events.
        /// When Paid, the webhook automatically transitions Status to Confirmed.
        /// Never set manually.
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        //──────────────────────────────────────────────

        /// <summary>
        /// Stripe PaymentIntent ID — set when BO initiates payment for the quoted price.
        /// Null until payment is initiated.
        /// </summary>
        public string? StripePaymentIntentId { get; set; }

        /// <summary>
        /// Payment lifecycle state driven by Stripe webhook events.
        /// When Paid, the webhook automatically transitions Status to Confirmed.
        /// Never set manually.
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        /// <summary>
        /// Estimated date by which production will be completed, set by Talentree when quoting.
        /// </summary>
        public DateTime? EstimatedCompletionDate { get; set; }

        /// <summary>
        /// Timestamp when production was marked as completed.
        /// Set by Admin when transitioning to <see cref="BoProductionRequestStatus.Completed"/>.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        // ── Navigation ────────────────────────────────────────────
        /// <summary>The individual item lines specifying what to produce.</summary>
        public ICollection<BoProductionRequestItem> Items { get; set; }
            = new List<BoProductionRequestItem>();

        /// <summary>
        /// Chronological log of every status change on this request.
        /// Records both BO actions (submit, confirm, cancel) and Admin actions (review, quote, etc.).
        /// </summary>
        public ICollection<BoProductionRequestStatusHistory> StatusHistory { get; set; }
            = new List<BoProductionRequestStatusHistory>();

        // AI/Analytics fields
        public int? FulfillmentTimeHours { get; set; }

        public bool IsFraudFlag { get; set; } = false;

        public float? FraudScore { get; set; }
    }
}