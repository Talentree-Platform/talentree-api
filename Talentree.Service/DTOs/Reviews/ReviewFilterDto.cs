namespace Talentree.Service.DTOs.Reviews
{
    public class ReviewFilterDto
    {
        public int? Rating { get; set; }
        public int? ProductId { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } // "date", "rating"
        public bool SortDescending { get; set; } = true;

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