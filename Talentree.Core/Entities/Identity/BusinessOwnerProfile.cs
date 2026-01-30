// Talentree.Core/Entities/Identity/BusinessOwnerProfile.cs

using Talentree.Core.Entities;

namespace Talentree.Core.Entities.Identity
{
    public enum BusinessOwnerStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class BusinessOwnerProfile : BaseEntity
    {
        // FK
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        // Business Info
        public string BusinessName { get; set; } = null!;
        public string BusinessDescription { get; set; } = null!;
        public string BusinessCategory { get; set; } = null!;
        public string TaxId { get; set; } = null!;

        // Approval workflow
        public BusinessOwnerStatus Status { get; set; } = BusinessOwnerStatus.Pending;

        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public string? RejectionReason { get; set; }

        // Auto escalation
        public DateTime? AutoApprovalDeadline { get; set; }
    }
}
