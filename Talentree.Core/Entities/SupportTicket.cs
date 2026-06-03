using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    public class SupportTicket : AuditableEntity, ISoftDelete
    {
        public string TicketNumber { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;

        public TicketCategory Category { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;

        // -------------- AI ---------------------
        public float? PriorityScore { get; set; } // 0.0 to 1.0
        public string? AutoCategory { get; set; }
        // ---------------------------------------
        public string BusinessOwnerUserId { get; set; } = null!;
        public AppUser BusinessOwner { get; set; } = null!;

        public string? AssignedToAdminId { get; set; }
        public DateTime? AssignedAt { get; set; }

        public int? RelatedSupplierId { get; set; }
        public Supplier? RelatedSupplier { get; set; }

        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedBy { get; set; }

        public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}