using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class SupportTicket : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public AppUser User { get; set; } = null!;

        public string UserType { get; set; } = null!;    // Customer / BusinessOwner
        public string Category { get; set; } = null!;   // Technical / Account / Payment / Other
        public string Subject { get; set; } = null!;
        public string Status { get; set; } = "Open";     // Open / InProgress / Resolved / Closed
        public string Priority { get; set; } = "Medium"; // Low / Medium / High / Urgent

        // AI fields
        public float? PriorityScore { get; set; }
        public string? AutoCategory { get; set; }

        public string? AssignedToAdminId { get; set; }

        // Navigation
        public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    }
}