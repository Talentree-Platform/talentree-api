namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    public class ArticleFilterParams
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? ContentType { get; set; }
        public string? Tag { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}