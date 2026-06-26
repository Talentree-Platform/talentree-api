using Microsoft.AspNetCore.Http;

namespace Talentree.Service.DTOs.Knowledge
{
    /// <summary>
    /// FR-AD-42: DTO for updating an existing article.
    /// All fields are nullable — only provided fields will be updated.
    /// </summary>
    public class UpdateArticleDto
    {
        // ── Core Metadata ─────────────────────────────────────────────
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Category { get; set; }
        public string? Tags { get; set; }
        public int? OrderIndex { get; set; }

        // ── Content (type-specific) ────────────────────────────────────
        public string? Content { get; set; }        // for Article type
        public string? ExternalUrl { get; set; }    // for Video type (YouTube link)

        // ── File Replacements ──────────────────────────────────────────
        public IFormFile? ThumbnailFile { get; set; }   // replace thumbnail
        public IFormFile? VideoFile { get; set; }       // replace video file
        public IFormFile? PdfFile { get; set; }         // replace PDF file

        // ── Publish State ──────────────────────────────────────────────
        // NOTE: Use dedicated publish/unpublish endpoints for status changes.
        // This field is accepted here for convenience (e.g. set draft while editing).
        public bool? IsPublished { get; set; }
    }
}
