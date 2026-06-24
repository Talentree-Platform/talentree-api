namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-43: Logs search terms entered in the Knowledge Base search box.
    /// Used for analytics to identify gaps in educational content.
    /// </summary>
    public class ContentSearchLog : BaseEntity
    {
        public string SearchTerm { get; set; } = null!;
        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = null!;

        // Navigation
        public AppUser User { get; set; } = null!;
    }
}
