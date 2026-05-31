using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Knowledge;

namespace Talentree.Service.Contracts
{
    public interface IKnowledgeService
    {
        // FR-BO-36: Browse articles
        Task<Pagination<ArticleDto>> GetArticlesAsync(string userId, ArticleFilterDto filter);

        // FR-BO-36: Get single article (also increments view count)
        Task<ArticleDto> GetArticleByIdAsync(int articleId, string userId);

        // FR-BO-36: Bookmark / unbookmark
        Task BookmarkArticleAsync(int articleId, string userId);
        Task UnbookmarkArticleAsync(int articleId, string userId);

        // FR-BO-36: Get bookmarked articles
        Task<List<ArticleDto>> GetBookmarkedArticlesAsync(string userId);

        // FR-BO-37: Get recommendations
        Task<List<RecommendedArticleDto>> GetRecommendationsAsync(string userId);
    }
}