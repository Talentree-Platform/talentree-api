namespace Talentree.Core.Specifications.KnowledgeSpecifications
{
    /// <summary>
    /// Filter parameters for admin content library queries.
    /// Unlike the BO filter, this supports filtering by status (all/published/draft/deleted)
    /// and sorting by views or date.
    /// </summary>
    public class AdminArticleFilterParams
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? ContentType { get; set; }

        /// <summary>all | published | draft | deleted</summary>
        public string Status { get; set; } = "all";

        /// <summary>views | date</summary>
        public string SortBy { get; set; } = "date";

        public bool SortDesc { get; set; } = true;

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
