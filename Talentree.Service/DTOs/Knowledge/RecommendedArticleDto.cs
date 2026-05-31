namespace Talentree.Service.DTOs.Knowledge
{
    public class RecommendedArticleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string ReasonLabel { get; set; } = string.Empty; // "Getting Started" / "Boost Your Sales" etc.
    }
}