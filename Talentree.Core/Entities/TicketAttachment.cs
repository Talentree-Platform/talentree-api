namespace Talentree.Core.Entities
{
    public class TicketAttachment : BaseEntity
    {
        public int TicketId { get; set; }
        public SupportTicket Ticket { get; set; } = null!;

        public int? MessageId { get; set; }
        public TicketMessage? Message { get; set; }

        public string FileName { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = null!;
    }
}