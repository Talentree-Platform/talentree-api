namespace Talentree.Service.DTOs.Knowledge
{
    public class ArticleDto
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
        public int ViewCount { get; set; }
        public bool IsBookmarked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}