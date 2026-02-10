// Talentree.Core/Entities/Identity/BusinessOwnerProfile.cs

using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities.Identity
{
    public class BusinessOwnerProfile : BaseEntity
    {
        // FK
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        // Business Info
        public string BusinessName { get; set; } = null!;
        public string BusinessDescription { get; set; } = null!;
        public string BusinessCategory { get; set; } = null!;

        // Optional fields
        public string? BusinessAddress { get; set; }
        public string? TaxId { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WebsiteLink { get; set; }

        // Approval workflow
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public string? RejectionReason { get; set; }

        // Auto escalation
        public DateTime? AutoApprovalDeadline { get; set; }
    }
}
