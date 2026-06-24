namespace Talentree.Core.Entities
{
    public class KnowledgeArticle : AuditableEntity, ISoftDelete
    {
        public string Title { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public string? Content { get; set; }        // for articles/guides
        public string? ExternalUrl { get; set; }    // for videos / external links
        public string? FileUrl { get; set; }        // for PDFs / videos
        public string ContentType { get; set; } = null!; // Video / PDF / Article
        public string Category { get; set; } = null!;    // Business / Craft / Marketing / PlatformGuide / GettingStarted / Inventory
        public string? Tags { get; set; }           // comma-separated e.g. "beginner,sales,products"
        public string? ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public long TotalViewDurationSeconds { get; set; } = 0; // FR-AD-43: cumulative view duration for avg calculation
        public int OrderIndex { get; set; } = 0;    // for manual ordering within category

        // FR-AD-42: Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation
        public ICollection<ArticleBookmark> Bookmarks { get; set; } = new List<ArticleBookmark>();
    }
}