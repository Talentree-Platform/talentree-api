using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.BoProductionRequest
{
    // ──────────────────────────────────────────────────────────
    // BO Input DTOs
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// The production request form submitted by a Business Owner to Talentree.
    /// Describes what the BO wants manufactured and which materials to use.
    /// </summary>
    public class SubmitProductionRequestDto
    {
        /// <summary>
        /// Short descriptive title for the request.
        /// Example: "Summer Collection Batch — Hoodies and Bags"
        /// </summary>
        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Optional free-text notes: deadlines, design preferences, special requirements.
        /// </summary>
        [MaxLength(3000)]
        public string? Notes { get; set; }

        /// <summary>
        /// The production item lines — at least one is required.
        /// Each line specifies a product type, quantity, and optional material preference.
        /// </summary>
        [Required, MinLength(1, ErrorMessage = "At least one production item is required.")]
        public List<ProductionRequestItemInputDto> Items { get; set; } = new();
    }

    /// <summary>
    /// A single line in the production request form.
    /// </summary>
    public class ProductionRequestItemInputDto
    {
        /// <summary>
        /// Type of product to manufacture. Free text.
        /// Example: "Hoodie", "Crossbody Bag", "Scented Candle"
        /// </summary>
        [Required, MaxLength(200)]
        public string ProductType { get; set; } = null!;

        /// <summary>Number of units to produce for this line.</summary>
        [Range(1, 100_000)]
        public int Quantity { get; set; }

        /// <summary>
        /// Optional: ID of a raw material from Talentree's store the BO wants used.
        /// Browse GET /api/raw-materials to find available materials.
        /// When null, Talentree selects the most suitable material.
        /// </summary>
        public int? PreferredRawMaterialId { get; set; }

        /// <summary>
        /// Optional item-level specifications: colour, size, finish, design reference, etc.
        /// </summary>
        [MaxLength(1000)]
        public string? Specifications { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Response DTOs
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// A summary row returned in the BO's production request history list.
    /// </summary>
    public class ProductionRequestSummaryDto
    {
        /// <summary>Request primary key.</summary>
        public int Id { get; set; }

        /// <summary>Title provided by the BO at submission.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Current lifecycle status of the request.</summary>
        public BoProductionRequestStatus Status { get; set; }

        /// <summary>When the request was submitted.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Price quoted by Talentree — null until an Admin sends a quote.
        /// </summary>
        public decimal? QuotedPrice { get; set; }

        /// <summary>
        /// Estimated production completion date — null until Admin quotes.
        /// </summary>
        public DateTime? EstimatedCompletionDate { get; set; }

        /// <summary>Number of distinct production item lines in this request.</summary>
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// Full detail of a single production request.
    /// Returned from the detail endpoint and after any status transition.
    /// </summary>
    public class ProductionRequestDetailDto
    {
        /// <summary>Request primary key.</summary>
        public int Id { get; set; }

        /// <summary>Title provided by the BO at submission.</summary>
        public string Title { get; set; } = null!;

        /// <summary>Optional notes provided by the BO at submission.</summary>
        public string? Notes { get; set; }

        /// <summary>Current lifecycle status.</summary>
        public BoProductionRequestStatus Status { get; set; }

        /// <summary>When the request was submitted.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Price quoted by Talentree — null until Admin sends a quote.</summary>
        public decimal? QuotedPrice { get; set; }

        /// <summary>
        /// Admin's message to the BO — quote breakdown, rejection reason, production updates, etc.
        /// </summary>
        public string? AdminNotes { get; set; }

        /// <summary>Estimated production completion date set by Talentree when quoting.</summary>
        public DateTime? EstimatedCompletionDate { get; set; }

        /// <summary>Actual timestamp when production was marked as completed.</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>All production item lines in this request.</summary>
        public List<ProductionRequestItemDetailDto> Items { get; set; } = new();

        /// <summary>Chronological status history showing every transition with its timestamp.</summary>
        public List<ProductionRequestHistoryDto> StatusHistory { get; set; } = new();
    }

    /// <summary>
    /// Detail for a single production item line inside a request.
    /// </summary>
    public class ProductionRequestItemDetailDto
    {
        /// <summary>Item line primary key.</summary>
        public int Id { get; set; }

        /// <summary>Product type as entered by the BO — e.g. "Hoodie".</summary>
        public string ProductType { get; set; } = null!;

        /// <summary>Number of units requested.</summary>
        public int Quantity { get; set; }

        /// <summary>Optional specifications: colour, size, finish, etc.</summary>
        public string? Specifications { get; set; }

        /// <summary>ID of the raw material preferred by the BO — null if left to Talentree.</summary>
        public int? PreferredRawMaterialId { get; set; }

        /// <summary>Display name of the preferred raw material — null if none selected.</summary>
        public string? PreferredRawMaterialName { get; set; }
    }

    /// <summary>
    /// A single entry in the status history timeline of a production request.
    /// </summary>
    public class ProductionRequestHistoryDto
    {
        /// <summary>The status that was applied at this point in time.</summary>
        public BoProductionRequestStatus Status { get; set; }

        /// <summary>When this status change occurred.</summary>
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// Optional contextual notes for this transition —
        /// e.g. rejection reason, quote summary, completion notes.
        /// </summary>
        public string? Notes { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Admin Action Input DTOs
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Payload for the Admin to send a price quote and estimated timeline to the BO.
    /// Transitions the request from <c>UnderReview</c> → <c>Quoted</c>.
    /// </summary>
    public class SendQuoteDto
    {
        /// <summary>Total price Talentree is quoting for this production run.</summary>
        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Quoted price must be greater than 0.")]
        public decimal QuotedPrice { get; set; }

        /// <summary>Estimated date by which Talentree will complete production.</summary>
        [Required]
        public DateTime EstimatedCompletionDate { get; set; }

        /// <summary>
        /// Optional message to the BO — quote breakdown, material choices, terms, etc.
        /// </summary>
        [MaxLength(2000)]
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// Payload for the Admin to mark production as completed.
    /// Transitions the request from <c>InProduction</c> → <c>Completed</c>.
    /// </summary>
    public class CompleteRequestDto
    {
        /// <summary>Optional closing notes — delivery info, collection instructions, etc.</summary>
        [MaxLength(2000)]
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// Payload for the Admin to reject a production request.
    /// A reason is mandatory so the BO understands why their request was declined.
    /// </summary>
    public class RejectRequestDto
    {
        /// <summary>Mandatory rejection reason — shown to the BO via AdminNotes.</summary>
        [Required, MaxLength(2000)]
        public string Reason { get; set; } = null!;
    }
}