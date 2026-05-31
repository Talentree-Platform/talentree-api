using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Support
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public TicketCategory Category { get; set; }
        public string CategoryText { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; }
        public string PriorityText { get; set; } = string.Empty;

        public string BusinessOwnerUserId { get; set; } = string.Empty;
        public string BusinessOwnerName { get; set; } = string.Empty;
        public string BusinessOwnerEmail { get; set; } = string.Empty;

        public string? AssignedToAdminId { get; set; }
        public DateTime? AssignedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedBy { get; set; }

        public int MessageCount { get; set; }
        public int AttachmentCount { get; set; }
        public List<TicketAttachmentDto> Attachments { get; set; } = new();
        public List<TicketMessageDto> Messages { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}