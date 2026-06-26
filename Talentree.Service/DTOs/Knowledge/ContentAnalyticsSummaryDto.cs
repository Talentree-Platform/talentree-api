namespace Talentree.Service.DTOs.Knowledge
{
    /// <summary>
    /// FR-AD-43: Dashboard-level analytics summary for the Content Library.
    /// </summary>
    public class ContentAnalyticsSummaryDto
    {
        // Totals
        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }
        public long TotalViews { get; set; }
        public long TotalBookmarks { get; set; }

        // Top performers
        public List<ArticleAnalyticsDto> MostViewedArticles { get; set; } = new();

        // Search intelligence
        public List<SearchTermDto> TopSearchTerms { get; set; } = new();
    }

    /// <summary>A search term with its frequency count.</summary>
    public class SearchTermDto
    {
        public string Term { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
