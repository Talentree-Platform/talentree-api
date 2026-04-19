using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    public class BookmarksByUserSpecification : BaseSpecifications<ArticleBookmark>
    {
        public BookmarksByUserSpecification(string userId)
            : base(b => b.UserId == userId)
        {
            AddInclude(b => b.Article);
        }
    }
}