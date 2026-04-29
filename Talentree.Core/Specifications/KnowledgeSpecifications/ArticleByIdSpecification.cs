using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    public class ArticleByIdSpecification : BaseSpecifications<KnowledgeArticle>
    {
        public ArticleByIdSpecification(int articleId)
            : base(a => a.Id == articleId && a.IsPublished)
        {
        }
    }
}