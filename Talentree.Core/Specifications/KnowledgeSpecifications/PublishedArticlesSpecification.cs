using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    public class PublishedArticlesSpecification : BaseSpecifications<KnowledgeArticle>
    {
        // Paginated + filtered
        public PublishedArticlesSpecification(ArticleFilterParams filter)
            : base(a =>
                a.IsPublished &&
                (string.IsNullOrEmpty(filter.Category) || a.Category == filter.Category) &&
                (string.IsNullOrEmpty(filter.ContentType) || a.ContentType == filter.ContentType) &&
                (string.IsNullOrEmpty(filter.Tag) || a.Tags != null && a.Tags.Contains(filter.Tag)) &&
                (string.IsNullOrEmpty(filter.Search) ||
                    a.Title.ToLower().Contains(filter.Search.ToLower()) ||
                    a.Summary.ToLower().Contains(filter.Search.ToLower()))
            )
        {
            AddOrderBy(a => (object)a.OrderIndex);
            ApplyPagination(filter.PageIndex, filter.PageSize);
        }

        // Count only
        public PublishedArticlesSpecification(ArticleFilterParams filter, bool countOnly)
            : base(a =>
                a.IsPublished &&
                (string.IsNullOrEmpty(filter.Category) || a.Category == filter.Category) &&
                (string.IsNullOrEmpty(filter.ContentType) || a.ContentType == filter.ContentType) &&
                (string.IsNullOrEmpty(filter.Tag) || a.Tags != null && a.Tags.Contains(filter.Tag)) &&
                (string.IsNullOrEmpty(filter.Search) ||
                    a.Title.ToLower().Contains(filter.Search.ToLower()) ||
                    a.Summary.ToLower().Contains(filter.Search.ToLower()))
            )
        {
        }
    }
}