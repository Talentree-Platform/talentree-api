using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Payout
{
    /// <summary>
    /// Submitted by the BO to request a withdrawal of their available balance.
    /// </summary>
    public class CreatePayoutRequestDto
    {
        /// <summary>Amount to withdraw in EGP. Must be ≤ available balance.</summary>
        [Required]
        [Range(100, double.MaxValue, ErrorMessage = "Minimum payout amount is 100 EGP.")]
        public decimal Amount { get; set; }

        [Required, MaxLength(150)]
        public string BankName { get; set; } = null!;

        [Required, MaxLength(150)]
        public string AccountHolderName { get; set; } = null!;

        /// <summary>Account number or IBAN — encrypted before storage.</summary>
        [Required, MaxLength(50)]
        public string AccountIdentifier { get; set; } = null!;

        [MaxLength(50)]
        public string? RoutingSwiftCode { get; set; }
    }

    /// <summary>
    /// Payout request detail returned to the BO or Admin.
    /// Account number is masked for security.
    /// </summary>
    public class PayoutRequestDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public PayoutStatus Status { get; set; }
        public string? BankName { get; set; }
        public string? AccountHolderName { get; set; }

        /// <summary>Account number with all but last 4 digits masked — e.g. "****1234".</summary>
        public string? MaskedAccountIdentifier { get; set; }

        public string? RoutingSwiftCode { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    /// <summary>Admin payload to reject a payout request.</summary>
    public class RejectPayoutDto
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}