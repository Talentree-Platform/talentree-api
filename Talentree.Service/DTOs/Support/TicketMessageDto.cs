namespace Talentree.Service.DTOs.Support
{
    public class TicketMessageDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public bool IsAdminMessage { get; set; }

        public string Content { get; set; } = string.Empty;

        public List<TicketAttachmentDto> Attachments { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}