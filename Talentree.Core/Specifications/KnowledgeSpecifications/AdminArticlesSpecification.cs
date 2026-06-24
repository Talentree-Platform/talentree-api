using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    /// <summary>
    /// FR-AD-40: Admin content library specification.
    /// Returns ALL articles regardless of IsPublished, with optional soft-delete filtering.
    /// NOTE: Bypasses the global soft-delete query filter by including IsDeleted in criteria.
    /// </summary>
    public class AdminArticlesSpecification : BaseSpecifications<KnowledgeArticle>
    {
        // Paginated + filtered
        public AdminArticlesSpecification(AdminArticleFilterParams filter)
            : base(a =>
                // Status filter
                (filter.Status == "all"       ? !a.IsDeleted :
                 filter.Status == "published" ? !a.IsDeleted && a.IsPublished :
                 filter.Status == "draft"     ? !a.IsDeleted && !a.IsPublished :
                 filter.Status == "deleted"   ? a.IsDeleted : !a.IsDeleted) &&

                // Category filter
                (string.IsNullOrEmpty(filter.Category) || a.Category == filter.Category) &&

                // ContentType filter
                (string.IsNullOrEmpty(filter.ContentType) || a.ContentType == filter.ContentType) &&

                // Title search
                (string.IsNullOrEmpty(filter.Search) ||
                    a.Title.ToLower().Contains(filter.Search.ToLower()) ||
                    a.Summary.ToLower().Contains(filter.Search.ToLower()))
            )
        {
            // Bypass global soft-delete query filter so deleted articles are visible
            EnableIgnoreQueryFilters();

            // Sorting
            if (filter.SortBy == "views")
            {
                if (filter.SortDesc)
                    AddOrderByDescending(a => (object)a.ViewCount);
                else
                    AddOrderBy(a => (object)a.ViewCount);
            }
            else // default: date
            {
                if (filter.SortDesc)
                    AddOrderByDescending(a => (object)a.CreatedAt);
                else
                    AddOrderBy(a => (object)a.CreatedAt);
            }

            ApplyPagination(filter.PageIndex, filter.PageSize);
        }

        // Count only (for pagination total)
        public AdminArticlesSpecification(AdminArticleFilterParams filter, bool countOnly)
            : base(a =>
                (filter.Status == "all"       ? !a.IsDeleted :
                 filter.Status == "published" ? !a.IsDeleted && a.IsPublished :
                 filter.Status == "draft"     ? !a.IsDeleted && !a.IsPublished :
                 filter.Status == "deleted"   ? a.IsDeleted : !a.IsDeleted) &&

                (string.IsNullOrEmpty(filter.Category) || a.Category == filter.Category) &&
                (string.IsNullOrEmpty(filter.ContentType) || a.ContentType == filter.ContentType) &&
                (string.IsNullOrEmpty(filter.Search) ||
                    a.Title.ToLower().Contains(filter.Search.ToLower()) ||
                    a.Summary.ToLower().Contains(filter.Search.ToLower()))
            )
        {
            EnableIgnoreQueryFilters();
        }
    }
}
