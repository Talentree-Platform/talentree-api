using FluentValidation;
using Talentree.Core.Enums;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.Service.Validators.Knowledge
{
    public class CreateArticleDtoValidator : AbstractValidator<CreateArticleDto>
    {
        private const long MaxVideoBytes = 100L * 1024 * 1024;  // 100 MB
        private const long MaxPdfBytes = 20L * 1024 * 1024;     // 20 MB
        private const long MaxThumbnailBytes = 5L * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/jpg", "image/webp" };
        private static readonly string[] AllowedVideoTypes = { "video/mp4", "video/webm", "video/avi", "video/mov", "video/quicktime" };
        private static readonly string[] AllowedPdfTypes = { "application/pdf" };

        public CreateArticleDtoValidator()
        {
            // ── Core Metadata ──────────────────────────────────────────────
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

            RuleFor(x => x.Summary)
                .NotEmpty().WithMessage("Summary is required.")
                .MaximumLength(500).WithMessage("Summary must not exceed 500 characters.");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("ContentType is required.")
                .Must(ContentType.IsValid)
                .WithMessage($"ContentType must be one of: {string.Join(", ", ContentType.AllValues)}.");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required.")
                .Must(ContentCategory.IsValid)
                .WithMessage($"Category must be one of: {string.Join(", ", ContentCategory.AllValues)}.");

            // ── Video-specific rules ────────────────────────────────────────
            When(x => x.ContentType == ContentType.Video, () =>
            {
                RuleFor(x => x)
                    .Must(x => !string.IsNullOrEmpty(x.ExternalUrl) || x.VideoFile != null)
                    .WithMessage("For Video content, either a YouTube/Vimeo URL or a video file upload is required.")
                    .Must(x => string.IsNullOrEmpty(x.ExternalUrl) || x.VideoFile == null)
                    .WithMessage("Provide either a URL or a video file, not both.");

                When(x => x.VideoFile != null, () =>
                {
                    RuleFor(x => x.VideoFile)
                        .Must(f => f!.Length <= MaxVideoBytes)
                        .WithMessage("Video file must not exceed 100 MB.")
                        .Must(f => AllowedVideoTypes.Contains(f!.ContentType.ToLower()))
                        .WithMessage("Video file must be MP4, WebM, AVI, or MOV.");
                });

                When(x => !string.IsNullOrEmpty(x.ExternalUrl), () =>
                {
                    RuleFor(x => x.ExternalUrl)
                        .Must(url => url!.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        .WithMessage("External URL must use HTTPS.");
                });
            });

            // ── PDF-specific rules ─────────────────────────────────────────
            When(x => x.ContentType == ContentType.PDF, () =>
            {
                RuleFor(x => x.PdfFile)
                    .NotNull().WithMessage("A PDF file is required for PDF content type.")
                    .Must(f => f!.Length <= MaxPdfBytes)
                    .WithMessage("PDF file must not exceed 20 MB.")
                    .Must(f => AllowedPdfTypes.Contains(f!.ContentType.ToLower()))
                    .WithMessage("Uploaded file must be a PDF.");
            });

            // ── Article-specific rules ─────────────────────────────────────
            When(x => x.ContentType == ContentType.Article, () =>
            {
                RuleFor(x => x.Content)
                    .NotEmpty().WithMessage("Article content body is required for Article content type.");
            });

            // ── Optional thumbnail rules ───────────────────────────────────
            When(x => x.ThumbnailFile != null, () =>
            {
                RuleFor(x => x.ThumbnailFile)
                    .Must(f => f!.Length <= MaxThumbnailBytes)
                    .WithMessage("Thumbnail image must not exceed 5 MB.")
                    .Must(f => AllowedImageTypes.Contains(f!.ContentType.ToLower()))
                    .WithMessage("Thumbnail must be JPEG, PNG, or WebP.");
            });
        }
    }
}
