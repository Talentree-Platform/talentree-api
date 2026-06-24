using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.Service.Contracts
{
    public interface IAdminKnowledgeService
    {
        // ═══════════════════════════════════════════════════════════
        // FR-AD-40: Content Library
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get paginated, filtered, sorted list of all articles (including drafts).</summary>
        Task<Pagination<AdminArticleDto>> GetAllArticlesAsync(AdminArticleFilterDto filter);

        /// <summary>Get single article details (including soft-deleted for admin review).</summary>
        Task<AdminArticleDto> GetArticleByIdAsync(int articleId);

        // ═══════════════════════════════════════════════════════════
        // FR-AD-41: Upload Content
        // ═══════════════════════════════════════════════════════════

        /// <summary>Create a new article. Handles file uploads and YouTube URL validation.</summary>
        Task<AdminArticleDto> CreateArticleAsync(CreateArticleDto dto, string adminId);

        // ═══════════════════════════════════════════════════════════
        // FR-AD-42: Edit / Delete / Restore
        // ═══════════════════════════════════════════════════════════

        /// <summary>Update metadata and/or replace files. Only provided fields are changed.</summary>
        Task<AdminArticleDto> UpdateArticleAsync(int articleId, UpdateArticleDto dto, string adminId);

        /// <summary>Publish a draft article — makes it visible to Business Owners.</summary>
        Task PublishArticleAsync(int articleId, string adminId);

        /// <summary>Unpublish a published article — hides it from Business Owners.</summary>
        Task UnpublishArticleAsync(int articleId, string adminId);

        /// <summary>Soft-delete: marks article as deleted but retains it in the database.</summary>
        Task DeleteArticleAsync(int articleId, string adminId);

        /// <summary>Restore a soft-deleted article back to draft state.</summary>
        Task RestoreArticleAsync(int articleId, string adminId);

        // ═══════════════════════════════════════════════════════════
        // FR-AD-43: Content Analytics
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get dashboard-level analytics summary (totals, top articles, top searches).</summary>
        Task<ContentAnalyticsSummaryDto> GetAnalyticsSummaryAsync();

        /// <summary>Get per-article analytics, sortable by views or date.</summary>
        Task<Pagination<ArticleAnalyticsDto>> GetArticleAnalyticsAsync(
            int pageIndex, int pageSize, string sortBy, bool sortDesc);

        // ═══════════════════════════════════════════════════════════
        // Duration Tracking (called from BO-side controller)
        // ═══════════════════════════════════════════════════════════

        /// <summary>Record how many seconds a BO user spent on an article (FR-AD-43).</summary>
        Task RecordViewDurationAsync(int articleId, int durationSeconds);
    }
}
