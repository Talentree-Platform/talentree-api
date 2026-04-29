using Microsoft.AspNetCore.Identity;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.KnowledgeSpecifications;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.Service.Services
{
    public class KnowledgeService : IKnowledgeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public KnowledgeService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Map filter DTO → params
        // ═══════════════════════════════════════════════════════════
        private static ArticleFilterParams MapToParams(ArticleFilterDto filter) => new()
        {
            Search = filter.Search,
            Category = filter.Category,
            ContentType = filter.ContentType,
            Tag = filter.Tag,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize
        };

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Map entity → DTO
        // ═══════════════════════════════════════════════════════════
        private static ArticleDto MapToDto(KnowledgeArticle a, bool isBookmarked = false)
        {
            return new ArticleDto
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
                ViewCount = a.ViewCount,
                IsBookmarked = isBookmarked,
                CreatedAt = a.CreatedAt
            };
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-36: Get Articles (paginated, filtered)
        // ═══════════════════════════════════════════════════════════
        public async Task<Pagination<ArticleDto>> GetArticlesAsync(string userId, ArticleFilterDto filter)
        {
            var filterParams = MapToParams(filter);

            var countSpec = new PublishedArticlesSpecification(filterParams, true);
            var totalCount = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new PublishedArticlesSpecification(filterParams);
            var articles = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetAllWithSpecificationsAsync(spec);

            // Get user bookmarks to mark IsBookmarked
            var bookmarkSpec = new BookmarksByUserSpecification(userId);
            var bookmarks = await _unitOfWork.Repository<ArticleBookmark>()
                .GetAllWithSpecificationsAsync(bookmarkSpec);
            var bookmarkedIds = bookmarks.Select(b => b.ArticleId).ToHashSet();

            var dtos = articles.Select(a => MapToDto(a, bookmarkedIds.Contains(a.Id))).ToList();

            return new Pagination<ArticleDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-36: Get Single Article + increment view count
        // ═══════════════════════════════════════════════════════════
        public async Task<ArticleDto> GetArticleByIdAsync(int articleId, string userId)
        {
            var spec = new ArticleByIdSpecification(articleId);
            var article = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new NotFoundException("Article not found");

            // Increment view count
            article.ViewCount++;
            _unitOfWork.Repository<KnowledgeArticle>().Update(article);
            await _unitOfWork.CompleteAsync();

            // Check if bookmarked
            var bookmarkSpec = new BookmarkByUserAndArticleSpecification(userId, articleId);
            var bookmark = await _unitOfWork.Repository<ArticleBookmark>()
                .GetByIdWithSpecificationsAsync(bookmarkSpec);

            return MapToDto(article, bookmark != null);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-36: Bookmark Article
        // ═══════════════════════════════════════════════════════════
        public async Task BookmarkArticleAsync(int articleId, string userId)
        {
            // Check article exists
            var spec = new ArticleByIdSpecification(articleId);
            var article = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new NotFoundException("Article not found");

            // Check not already bookmarked
            var bookmarkSpec = new BookmarkByUserAndArticleSpecification(userId, articleId);
            var existing = await _unitOfWork.Repository<ArticleBookmark>()
                .GetByIdWithSpecificationsAsync(bookmarkSpec);

            if (existing != null)
                throw new BadRequestException("Article is already bookmarked");

            _unitOfWork.Repository<ArticleBookmark>().Add(new ArticleBookmark
            {
                UserId = userId,
                ArticleId = articleId
            });

            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-36: Unbookmark Article
        // ═══════════════════════════════════════════════════════════
        public async Task UnbookmarkArticleAsync(int articleId, string userId)
        {
            var bookmarkSpec = new BookmarkByUserAndArticleSpecification(userId, articleId);
            var bookmark = await _unitOfWork.Repository<ArticleBookmark>()
                .GetByIdWithSpecificationsAsync(bookmarkSpec)
                ?? throw new NotFoundException("Bookmark not found");

            _unitOfWork.Repository<ArticleBookmark>().Delete(bookmark);
            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-36: Get Bookmarked Articles
        // ═══════════════════════════════════════════════════════════
        public async Task<List<ArticleDto>> GetBookmarkedArticlesAsync(string userId)
        {
            var spec = new BookmarksByUserSpecification(userId);
            var bookmarks = await _unitOfWork.Repository<ArticleBookmark>()
                .GetAllWithSpecificationsAsync(spec);

            return bookmarks
                .Where(b => b.Article != null && b.Article.IsPublished)
                .Select(b => MapToDto(b.Article, true))
                .ToList();
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-37: Get Recommendations
        // Logic:
        //   - New user (no products)      → GettingStarted
        //   - Has products, low reviews   → Marketing
        //   - Has many products           → Inventory
        //   - Default                     → Platform updates
        // ═══════════════════════════════════════════════════════════
        public async Task<List<RecommendedArticleDto>> GetRecommendationsAsync(string userId)
        {
            // Get business owner profile
            var profileSpec = new BusinessOwnerProfileByUserIdSpecification(userId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(profileSpec);

            string targetCategory;
            string reasonLabel;

            if (profile == null || !profile.Products.Any())
            {
                targetCategory = "GettingStarted";
                reasonLabel = "Getting Started";
            }
            else if (profile.Products.Count > 10)
            {
                targetCategory = "Inventory";
                reasonLabel = "Manage Your Inventory";
            }
            else
            {
                targetCategory = "Marketing";
                reasonLabel = "Boost Your Sales";
            }

            var filterParams = new ArticleFilterParams
            {
                Category = targetCategory,
                PageIndex = 1,
                PageSize = 3
            };

            var spec = new PublishedArticlesSpecification(filterParams);
            var articles = await _unitOfWork.Repository<KnowledgeArticle>()
                .GetAllWithSpecificationsAsync(spec);

            return articles.Select(a => new RecommendedArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                ContentType = a.ContentType,
                Category = a.Category,
                ThumbnailUrl = a.ThumbnailUrl,
                ReasonLabel = reasonLabel
            }).ToList();
        }
    }
}