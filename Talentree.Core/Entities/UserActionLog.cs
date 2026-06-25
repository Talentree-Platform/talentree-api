using System;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class UserActionLog : BaseEntity
    {
        public string? UserId { get; set; }
        public string? AdminId { get; set; }
        public string Action { get; set; } = null!; // Suspend, Ban, Block, Unblock, etc.
        public string Reason { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Audit enhancement fields
        public string? IpAddress { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? BeforeValues { get; set; }
        public string? AfterValues { get; set; }

        // Navigation
        public AppUser? User { get; set; }
        public AppUser? Admin { get; set; }
    }
}