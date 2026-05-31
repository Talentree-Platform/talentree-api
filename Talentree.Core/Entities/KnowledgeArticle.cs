namespace Talentree.Core.Entities
{
    public class KnowledgeArticle : AuditableEntity
    {
        public string Title { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public string? Content { get; set; }        // for articles/guides
        public string? ExternalUrl { get; set; }    // for videos / external links
        public string? FileUrl { get; set; }        // for PDFs
        public string ContentType { get; set; } = null!; // Guide / Video / Article / Tip / Update
        public string Category { get; set; } = null!;    // GettingStarted / Marketing / Inventory / Platform
        public string? Tags { get; set; }           // comma-separated e.g. "beginner,sales,products"
        public string? ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public int OrderIndex { get; set; } = 0;    // for manual ordering within category

        // Navigation
        public ICollection<ArticleBookmark> Bookmarks { get; set; } = new List<ArticleBookmark>();
    }
}