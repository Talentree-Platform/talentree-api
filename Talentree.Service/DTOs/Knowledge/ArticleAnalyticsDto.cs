namespace Talentree.Service.DTOs.Knowledge
{
    /// <summary>
    /// FR-AD-43: Per-article analytics data.
    /// </summary>
    public class ArticleAnalyticsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsPublished { get; set; }

        // View metrics
        public int ViewCount { get; set; }
        public long TotalViewDurationSeconds { get; set; }

        /// <summary>Calculated as TotalViewDurationSeconds / ViewCount (0 if no views)</summary>
        public double AvgViewDurationSeconds { get; set; }

        // Engagement
        public int BookmarkCount { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
