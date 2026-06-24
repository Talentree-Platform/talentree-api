namespace Talentree.Service.DTOs.Knowledge
{
    /// <summary>
    /// FR-AD-40: Query parameters for admin content library listing.
    /// </summary>
    public class AdminArticleFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? ContentType { get; set; }

        /// <summary>all | published | draft | deleted</summary>
        public string Status { get; set; } = "all";

        /// <summary>views | date</summary>
        public string SortBy { get; set; } = "date";

        public bool SortDesc { get; set; } = true;

        private int _pageIndex = 1;
        private int _pageSize = 20;

        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
        }
    }
}
