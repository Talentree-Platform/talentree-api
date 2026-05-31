namespace Talentree.Service.DTOs.Knowledge
{
    public class ArticleFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? ContentType { get; set; }
        public string? Tag { get; set; }

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