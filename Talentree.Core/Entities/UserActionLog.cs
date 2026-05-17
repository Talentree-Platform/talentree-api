namespace Talentree.Core.Entities
{
    public class UserActionLog : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public string AdminId { get; set; } = null!;
        public string Action { get; set; } = null!; // Suspend, Ban, Block, Unblock, etc.
        public string Reason { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public AppUser User { get; set; } = null!;
        public AppUser Admin { get; set; } = null!;
    }
}