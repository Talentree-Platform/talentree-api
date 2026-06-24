using FluentValidation;
using Talentree.Core.Enums;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.Service.Validators.Knowledge
{
    public class UpdateArticleDtoValidator : AbstractValidator<UpdateArticleDto>
    {
        private const long MaxVideoBytes = 100L * 1024 * 1024;   // 100 MB
        private const long MaxPdfBytes = 20L * 1024 * 1024;      // 20 MB
        private const long MaxThumbnailBytes = 5L * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/jpg", "image/webp" };
        private static readonly string[] AllowedVideoTypes = { "video/mp4", "video/webm", "video/avi", "video/mov", "video/quicktime" };
        private static readonly string[] AllowedPdfTypes = { "application/pdf" };

        public UpdateArticleDtoValidator()
        {
            // ── Optional fields — validate only when provided ──────────────
            When(x => x.Title != null, () =>
            {
                RuleFor(x => x.Title)
                    .NotEmpty().WithMessage("Title cannot be empty.")
                    .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");
            });

            When(x => x.Summary != null, () =>
            {
                RuleFor(x => x.Summary)
                    .NotEmpty().WithMessage("Summary cannot be empty.")
                    .MaximumLength(500).WithMessage("Summary must not exceed 500 characters.");
            });

            When(x => x.Category != null, () =>
            {
                RuleFor(x => x.Category)
                    .Must(ContentCategory.IsValid)
                    .WithMessage($"Category must be one of: {string.Join(", ", ContentCategory.AllValues)}.");
            });

            When(x => x.ExternalUrl != null, () =>
            {
                RuleFor(x => x.ExternalUrl)
                    .Must(url => url!.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("External URL must use HTTPS.");
            });

            // ── File replacement validations ───────────────────────────────
            When(x => x.ThumbnailFile != null, () =>
            {
                RuleFor(x => x.ThumbnailFile)
                    .Must(f => f!.Length <= MaxThumbnailBytes)
                    .WithMessage("Thumbnail image must not exceed 5 MB.")
                    .Must(f => AllowedImageTypes.Contains(f!.ContentType.ToLower()))
                    .WithMessage("Thumbnail must be JPEG, PNG, or WebP.");
            });

            When(x => x.VideoFile != null, () =>
            {
                RuleFor(x => x.VideoFile)
                    .Must(f => f!.Length <= MaxVideoBytes)
                    .WithMessage("Video file must not exceed 100 MB.")
                    .Must(f => AllowedVideoTypes.Contains(f!.ContentType.ToLower()))
                    .WithMessage("Video file must be MP4, WebM, AVI, or MOV.");
            });

            When(x => x.PdfFile != null, () =>
            {
                RuleFor(x => x.PdfFile)
                    .Must(f => f!.Length <= MaxPdfBytes)
                    .WithMessage("PDF file must not exceed 20 MB.")
                    .Must(f => AllowedPdfTypes.Contains(f!.ContentType.ToLower()))
                    .WithMessage("Uploaded file must be a PDF.");
            });
        }
    }
}
