using Microsoft.AspNetCore.Http;

namespace Talentree.Service.DTOs.Knowledge
{
    /// <summary>
    /// FR-AD-41: DTO for creating a new educational content item.
    /// Supports all three content types: Video (file or YouTube URL), PDF, and Article.
    /// </summary>
    public class CreateArticleDto
    {
        // ── Core Metadata ────────────────────────────────────────────
        public string Title { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public string ContentType { get; set; } = null!;    // Video | PDF | Article
        public string Category { get; set; } = null!;       // Business | Craft | Marketing | PlatformGuide | GettingStarted | Inventory
        public string? Tags { get; set; }                   // comma-separated, e.g. "beginner,sales"
        public int OrderIndex { get; set; } = 0;
        public bool IsPublished { get; set; } = false;      // false = Draft

        // ── Content (type-specific, at least one required) ───────────
        public string? Content { get; set; }                // Required for Article type (rich text / HTML)
        public string? ExternalUrl { get; set; }            // Required for Video type (YouTube / Vimeo link)

        // ── File Uploads ─────────────────────────────────────────────
        public IFormFile? ThumbnailFile { get; set; }       // Optional thumbnail image (≤ 5 MB)
        public IFormFile? VideoFile { get; set; }           // Optional video file (≤ 100 MB)
        public IFormFile? PdfFile { get; set; }             // Required for PDF type (≤ 20 MB)
    }
}
