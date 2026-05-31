using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    public class BookmarkByUserAndArticleSpecification : BaseSpecifications<ArticleBookmark>
    {
        public BookmarkByUserAndArticleSpecification(string userId, int articleId)
            : base(b => b.UserId == userId && b.ArticleId == articleId)
        {
        }
    }
}