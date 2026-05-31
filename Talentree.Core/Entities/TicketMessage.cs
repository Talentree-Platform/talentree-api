using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class TicketMessage : AuditableEntity
    {
        public int TicketId { get; set; }
        public SupportTicket Ticket { get; set; } = null!;

        public string SenderId { get; set; } = null!;
        public AppUser Sender { get; set; } = null!;

        public string Content { get; set; } = null!;
        public bool IsAdminMessage { get; set; }
        public bool EmailSent { get; set; } = false;

        public ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    }
}