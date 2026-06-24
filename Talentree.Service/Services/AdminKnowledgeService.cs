using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.KnowledgeSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.Service.Services
{
    public class AdminKnowledgeService : IAdminKnowledgeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IFileService _fileService;

        public AdminKnowledgeService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            IImageService imageService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _imageService = imageService;
            _fileService = fileService;
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════

        private static AdminArticleDto MapToAdminDto(KnowledgeArticle a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Summary = a.Summary,
            Content = a.Content,
            ExternalUrl = a.ExternalUrl,
            FileUrl = a.FileUrl,
            ContentType = a.ContentType,
            Category = a.Category,
            Tags = string.IsNullOrEmpty(a.Tags)
                ? new List<string>()
                : a.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            ThumbnailUrl = a.ThumbnailUrl,
            IsPublished = a.IsPublished,
            ViewCount = a.ViewCount,
            TotalViewDurationSeconds = a.TotalViewDurationSeconds,
            OrderIndex = a.OrderIndex,
            IsDeleted = a.IsDeleted,
            DeletedAt = a.DeletedAt,
            DeletedBy = a.DeletedBy,
            CreatedBy = a.CreatedBy,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            UpdatedBy = a.UpdatedBy,
            BookmarkCount = a.Bookmarks?.Count ?? 0
        };

        private static AdminArticleFilterParams MapToParams(AdminArticleFilterDto filter) => new()
        {
            Search = filter.Search,
            Category = filter.Category,
            ContentType = filter.ContentType,
            Status = filter.Status,
            SortBy = filter.SortBy,
            SortDesc = filter.SortDesc,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize
        };

        private async Task<KnowledgeArticle> GetArticleOrThrowAsync(int articleId)
        {
            var spec = new ArticleByIdAdminSpecification(articleId);
            return await _unitOfWork.Repository<KnowledgeArticle>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new NotFoundException($"Article with ID {articleId} not found.");
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-40: Get All Articles (paginated, filtered, sorted)
        // ═══════════════════════════════════════════════════════════
        public async Task<Pagination<AdminArticleDto>> GetAllArticlesAsync(AdminArticleFilterDto filter)
        {
            var filterParams = MapToParams(filter);

            var countSpec = new AdminArticlesSpecification(filterParams, true);
            var totalCount = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new AdminArticlesSpecification(filterParams);
            var articles = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = articles.Select(MapToAdminDto).ToList();
            return new Pagination<AdminArticleDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-40: Get Single Article
        // ═══════════════════════════════════════════════════════════
        public async Task<AdminArticleDto> GetArticleByIdAsync(int articleId)
        {
            var article = await GetArticleOrThrowAsync(articleId);
            return MapToAdminDto(article);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-41: Create Article
        // ═══════════════════════════════════════════════════════════
        public async Task<AdminArticleDto> CreateArticleAsync(CreateArticleDto dto, string adminId)
        {
            string? thumbnailUrl = null;
            string? fileUrl = null;

            // Handle thumbnail upload
            if (dto.ThumbnailFile != null)
                thumbnailUrl = await _imageService.UploadImageAsync(dto.ThumbnailFile, "knowledge/thumbnails");

            // Handle content-type-specific file uploads
            if (dto.ContentType == Core.Enums.ContentType.PDF && dto.PdfFile != null)
                fileUrl = await _fileService.UploadFileAsync(dto.PdfFile, "knowledge/pdfs");

            if (dto.ContentType == Core.Enums.ContentType.Video && dto.VideoFile != null)
                fileUrl = await _fileService.UploadFileAsync(dto.VideoFile, "knowledge/videos");

            var article = new KnowledgeArticle
            {
                Title = dto.Title,
                Summary = dto.Summary,
                ContentType = dto.ContentType,
                Category = dto.Category,
                Tags = dto.Tags,
                Content = dto.Content,
                ExternalUrl = dto.ExternalUrl,
                FileUrl = fileUrl,
                ThumbnailUrl = thumbnailUrl,
                IsPublished = dto.IsPublished,
                OrderIndex = dto.OrderIndex,
                CreatedBy = adminId
            };

            _unitOfWork.Repository<KnowledgeArticle>().Add(article);
            await _unitOfWork.CompleteAsync();

            return MapToAdminDto(article);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-42: Update Article (partial update)
        // ═══════════════════════════════════════════════════════════
        public async Task<AdminArticleDto> UpdateArticleAsync(int articleId, UpdateArticleDto dto, string adminId)
        {
            var article = await GetArticleOrThrowAsync(articleId);

            if (article.IsDeleted)
                throw new BadRequestException("Cannot update a deleted article. Restore it first.");

            // ── Update metadata fields (only if provided) ──────────────────
            if (dto.Title != null) article.Title = dto.Title;
            if (dto.Summary != null) article.Summary = dto.Summary;
            if (dto.Category != null) article.Category = dto.Category;
            if (dto.Tags != null) article.Tags = dto.Tags;
            if (dto.Content != null) article.Content = dto.Content;
            if (dto.ExternalUrl != null) article.ExternalUrl = dto.ExternalUrl;
            if (dto.OrderIndex.HasValue) article.OrderIndex = dto.OrderIndex.Value;
            if (dto.IsPublished.HasValue) article.IsPublished = dto.IsPublished.Value;

            // ── Replace thumbnail ──────────────────────────────────────────
            if (dto.ThumbnailFile != null)
            {
                if (!string.IsNullOrEmpty(article.ThumbnailUrl))
                    await _imageService.DeleteImageAsync(article.ThumbnailUrl);

                article.ThumbnailUrl = await _imageService.UploadImageAsync(
                    dto.ThumbnailFile, "knowledge/thumbnails");
            }

            // ── Replace PDF file ───────────────────────────────────────────
            if (dto.PdfFile != null)
            {
                if (!string.IsNullOrEmpty(article.FileUrl))
                    await _fileService.DeleteFileAsync(article.FileUrl);

                article.FileUrl = await _fileService.UploadFileAsync(dto.PdfFile, "knowledge/pdfs");
                article.ExternalUrl = null; // clear URL if switching to file
            }

            // ── Replace video file ─────────────────────────────────────────
            if (dto.VideoFile != null)
            {
                if (!string.IsNullOrEmpty(article.FileUrl))
                    await _fileService.DeleteFileAsync(article.FileUrl);

                article.FileUrl = await _fileService.UploadFileAsync(dto.VideoFile, "knowledge/videos");
                article.ExternalUrl = null; // clear URL if switching to file
            }

            article.UpdatedBy = adminId;
            _unitOfWork.Repository<KnowledgeArticle>().Update(article);
            await _unitOfWork.CompleteAsync();

            return MapToAdminDto(article);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-42: Publish Article
        // ═══════════════════════════════════════════════════════════
        public async Task PublishArticleAsync(int articleId, string adminId)
        {
            var article = await GetArticleOrThrowAsync(articleId);

            if (article.IsDeleted)
                throw new BadRequestException("Cannot publish a deleted article. Restore it first.");

            if (article.IsPublished)
                throw new BadRequestException("Article is already published.");

            article.IsPublished = true;
            article.UpdatedBy = adminId;

            _unitOfWork.Repository<KnowledgeArticle>().Update(article);
            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-42: Unpublish Article
        // ═══════════════════════════════════════════════════════════
        public async Task UnpublishArticleAsync(int articleId, string adminId)
        {
            var article = await GetArticleOrThrowAsync(articleId);

            if (!article.IsPublished)
                throw new BadRequestException("Article is already unpublished.");

            article.IsPublished = false;
            article.UpdatedBy = adminId;

            _unitOfWork.Repository<KnowledgeArticle>().Update(article);
            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-42: Soft Delete
        // ═══════════════════════════════════════════════════════════
        public async Task DeleteArticleAsync(int articleId, string adminId)
        {
            var article = await GetArticleOrThrowAsync(articleId);

            if (article.IsDeleted)
                throw new BadRequestException("Article is already deleted.");

            article.IsDeleted = true;
            article.DeletedAt = DateTime.UtcNow;
            article.DeletedBy = adminId;
            article.IsPublished = false; // auto-unpublish on delete

            _unitOfWork.Repository<KnowledgeArticle>().Update(article);
            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-42: Restore Soft-Deleted Article
        // ═══════════════════════════════════════════════════════════
        public async Task RestoreArticleAsync(int articleId, string adminId)
        {
            var article = await GetArticleOrThrowAsync(articleId);

            if (!article.IsDeleted)
                throw new BadRequestException("Article is not deleted.");

            article.IsDeleted = false;
            article.DeletedAt = null;
            article.DeletedBy = null;
            article.IsPublished = false; // restored as draft — admin must explicitly publish
            article.UpdatedBy = adminId;

            _unitOfWork.Repository<KnowledgeArticle>().Update(article);
            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-43: Analytics Summary
        // ═══════════════════════════════════════════════════════════
        public async Task<ContentAnalyticsSummaryDto> GetAnalyticsSummaryAsync()
        {
            // Get all non-deleted articles (bypass soft-delete filter for admin)
            var allFilter = new AdminArticleFilterParams { Status = "all", PageSize = int.MaxValue };
            var allSpec = new AdminArticlesSpecification(allFilter);
            var allArticles = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetAllWithSpecificationsAsync(allSpec);

            var totalViews = allArticles.Sum(a => (long)a.ViewCount);
            var totalBookmarks = allArticles.Sum(a => (long)(a.Bookmarks?.Count ?? 0));

            // Top 5 most viewed published articles
            var topArticles = allArticles
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.ViewCount)
                .Take(5)
                .Select(a => new ArticleAnalyticsDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    ContentType = a.ContentType,
                    Category = a.Category,
                    IsPublished = a.IsPublished,
                    ViewCount = a.ViewCount,
                    TotalViewDurationSeconds = a.TotalViewDurationSeconds,
                    AvgViewDurationSeconds = a.ViewCount > 0
                        ? Math.Round((double)a.TotalViewDurationSeconds / a.ViewCount, 1)
                        : 0,
                    BookmarkCount = a.Bookmarks?.Count ?? 0,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList();

            // Top 10 most recent search terms
            var searchLogs = await _unitOfWork.Repository<ContentSearchLog>()
                .FindAsync(_ => true);

            var topSearchTerms = searchLogs
                .GroupBy(s => s.SearchTerm.ToLower().Trim())
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new SearchTermDto { Term = g.Key, Count = g.Count() })
                .ToList();

            return new ContentAnalyticsSummaryDto
            {
                TotalArticles = allArticles.Count(a => !a.IsDeleted),
                PublishedArticles = allArticles.Count(a => !a.IsDeleted && a.IsPublished),
                DraftArticles = allArticles.Count(a => !a.IsDeleted && !a.IsPublished),
                TotalViews = totalViews,
                TotalBookmarks = totalBookmarks,
                MostViewedArticles = topArticles,
                TopSearchTerms = topSearchTerms
            };
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-43: Per-Article Analytics
        // ═══════════════════════════════════════════════════════════
        public async Task<Pagination<ArticleAnalyticsDto>> GetArticleAnalyticsAsync(
            int pageIndex, int pageSize, string sortBy, bool sortDesc)
        {
            var filterParams = new AdminArticleFilterParams
            {
                Status = "all",
                SortBy = sortBy,
                SortDesc = sortDesc,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var countSpec = new AdminArticlesSpecification(filterParams, true);
            var totalCount = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new AdminArticlesSpecification(filterParams);
            var articles = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = articles.Select(a => new ArticleAnalyticsDto
            {
                Id = a.Id,
                Title = a.Title,
                ContentType = a.ContentType,
                Category = a.Category,
                IsPublished = a.IsPublished,
                ViewCount = a.ViewCount,
                TotalViewDurationSeconds = a.TotalViewDurationSeconds,
                AvgViewDurationSeconds = a.ViewCount > 0
                    ? Math.Round((double)a.TotalViewDurationSeconds / a.ViewCount, 1)
                    : 0,
                BookmarkCount = a.Bookmarks?.Count ?? 0,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();

            return new Pagination<ArticleAnalyticsDto>(pageIndex, pageSize, totalCount, dtos);
        }

        // ═══════════════════════════════════════════════════════════
        // Duration Tracking (BO-side, FR-AD-43)
        // ═══════════════════════════════════════════════════════════
        public async Task RecordViewDurationAsync(int articleId, int durationSeconds)
        {
            if (durationSeconds <= 0) return;

            var spec = new ArticleByIdSpecification(articleId);
            var article = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new NotFoundException($"Article with ID {articleId} not found.");

            article.TotalViewDurationSeconds += durationSeconds;
            _unitOfWork.Repository<KnowledgeArticle>().Update(article);
            await _unitOfWork.CompleteAsync();
        }
    }
}
