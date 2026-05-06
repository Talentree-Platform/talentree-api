namespace Talentree.Core.Entities
{
    public class AutoBlockLog : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public string Reason { get; set; } = null!; // "3 confirmed complaints", "Inactive 12 months", etc.
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

        public bool IsReviewed { get; set; } = false;
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminDecision { get; set; } // Maintain, Warn, Unblock
        public string? AdminNotes { get; set; }

        // Navigation
        public AppUser User { get; set; } = null!;
    }
}