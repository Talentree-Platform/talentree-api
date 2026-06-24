namespace Talentree.Service.DTOs.Knowledge
{
    /// <summary>
    /// FR-AD-40: Full article DTO returned to admin — includes status, analytics, and soft-delete info.
    /// </summary>
    public class AdminArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? ExternalUrl { get; set; }
        public string? FileUrl { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string? ThumbnailUrl { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public long TotalViewDurationSeconds { get; set; }
        public int OrderIndex { get; set; }

        // Soft delete info
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Audit
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Analytics extras
        public int BookmarkCount { get; set; }
    }
}
