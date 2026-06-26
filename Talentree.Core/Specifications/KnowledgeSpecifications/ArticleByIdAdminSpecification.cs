using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    /// <summary>
    /// FR-AD-42: Admin specification to fetch a single article by ID,
    /// including soft-deleted articles (admins can view and restore deleted content).
    /// </summary>
    public class ArticleByIdAdminSpecification : BaseSpecifications<KnowledgeArticle>
    {
        public ArticleByIdAdminSpecification(int articleId)
            : base(a => a.Id == articleId)
        {
            // Bypass global soft-delete filter — admin can see deleted articles
            EnableIgnoreQueryFilters();

            // Include bookmarks for bookmark count analytics
            AddInclude(a => (object)a.Bookmarks);
        }
    }
}
