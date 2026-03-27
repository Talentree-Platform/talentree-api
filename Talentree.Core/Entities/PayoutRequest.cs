using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// A request from a Business Owner to withdraw their available balance.
    /// Admin reviews and processes these manually in MVP.
    /// Only one pending request allowed per BO at a time (enforced in service).
    /// </summary>
    public class PayoutRequest : AuditableEntity
    {
        /// <summary>Identity ID of the Business Owner requesting the payout.</summary>
        public string BusinessOwnerId { get; set; } = null!;

        /// <summary>Amount the BO wants to withdraw in EGP.</summary>
        public decimal Amount { get; set; }

        /// <summary>Currency — always EGP for MVP.</summary>
        public string Currency { get; set; } = "EGP";

        /// <summary>Current processing status of the payout request.</summary>
        public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

        // ── Bank details (provided by BO at request time) ─────

        /// <summary>Name of the BO's bank.</summary>
        public string? BankName { get; set; }

        /// <summary>Full legal name of the account holder.</summary>
        public string? AccountHolderName { get; set; }

        /// <summary>
        /// Encrypted bank account number or IBAN.
        /// Must be encrypted at rest — never store plaintext.
        /// Displayed masked in API responses.
        /// </summary>
        public string? AccountIdentifierEnc { get; set; }

        /// <summary>Bank routing number or SWIFT/BIC code for international transfers.</summary>
        public string? RoutingSwiftCode { get; set; }

        // ── Admin response ────────────────────────────────────

        /// <summary>
        /// Reason provided by Admin when rejecting a payout request.
        /// Required when Status = Rejected.
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>Timestamp when Admin approved or rejected the request.</summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>Identity ID of the Admin who processed this request.</summary>
        public string? ProcessedBy { get; set; }
    }
}