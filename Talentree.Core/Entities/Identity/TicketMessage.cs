namespace Talentree.Core.Entities
{
    public class TicketMessage : BaseEntity
    {
        public int TicketId { get; set; }
        public SupportTicket Ticket { get; set; } = null!;

        public string SenderType { get; set; } = null!; // Customer / BusinessOwner / Admin
        public string Message { get; set; } = null!;
        public string? AttachmentUrl { get; set; }
    }
}