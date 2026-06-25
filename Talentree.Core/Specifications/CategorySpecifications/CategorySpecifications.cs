using Talentree.Core.Entities;
using Talentree.Core.Specifications;

namespace Talentree.Core.Specifications.CategorySpecifications
{
    /// <summary>
    /// FR-AD-31: Fetches all non-deleted categories and eagerly loads their subcategories.
    /// Root categories are filtered at the service layer.
    /// </summary>
    public class AllCategoriesWithSubcategoriesSpecification : BaseSpecifications<Category>
    {
        public AllCategoriesWithSubcategoriesSpecification() : base()
        {
            AddInclude(c => c.SubCategories);
            AddOrderBy(c => c.DisplayOrder);
        }
    }

    /// <summary>FR-AD-31: Fetches a single category by ID with its subcategories eagerly loaded.</summary>
    public class CategoryByIdWithSubcategoriesSpecification : BaseSpecifications<Category>
    {
        public CategoryByIdWithSubcategoriesSpecification(int id)
            : base(c => c.Id == id)
        {
            AddInclude(c => c.SubCategories);
        }
    }

    /// <summary>FR-AD-31: Fetches a batch of categories by their IDs (used for reorder operations).</summary>
    public class CategoriesByIdsSpecification : BaseSpecifications<Category>
    {
        public CategoriesByIdsSpecification(IEnumerable<int> ids)
            : base(c => ids.Contains(c.Id))
        {
        }
    }
}
