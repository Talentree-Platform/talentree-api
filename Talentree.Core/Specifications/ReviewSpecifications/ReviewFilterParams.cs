namespace Talentree.Core.Specifications.ReviewSpecifications
{
    public class ReviewFilterParams
    {
        public int? Rating { get; set; }        // filter by 1-5
        public int? ProductId { get; set; }     // filter by product
        public string? Search { get; set; }     // search in review text
        public string? SortBy { get; set; }     // "date", "rating"
        public bool SortDescending { get; set; } = true;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}