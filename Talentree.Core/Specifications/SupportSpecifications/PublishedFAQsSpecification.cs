using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.SupportSpecifications
{
    public class PublishedFAQsSpecification : BaseSpecifications<FAQ>
    {
        public PublishedFAQsSpecification(string? category = null, string? searchQuery = null)
            : base(f =>
                f.IsPublished &&
                (string.IsNullOrEmpty(category) || f.Category == category) &&
                (string.IsNullOrEmpty(searchQuery) ||
                 f.Question.Contains(searchQuery) ||
                 f.Answer.Contains(searchQuery) ||
                 (f.Keywords != null && f.Keywords.Contains(searchQuery))))
        {
            AddOrderBy(f => f.Category);
            AddOrderBy(f => f.DisplayOrder);
        }
    }
}