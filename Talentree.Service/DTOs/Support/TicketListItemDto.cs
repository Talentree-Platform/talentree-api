using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Support
{
    public class TicketListItemDto
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;

        public TicketCategory Category { get; set; }
        public string CategoryText { get; set; } = string.Empty;

        public TicketStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; }
        public string PriorityText { get; set; } = string.Empty;

        public int MessageCount { get; set; }
        public bool HasUnreadMessages { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}