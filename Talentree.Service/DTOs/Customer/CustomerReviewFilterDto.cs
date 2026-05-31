namespace Talentree.Service.DTOs.Customer
{
    public class CustomerReviewFilterDto
    {
        public int? Rating { get; set; }
        public string? SortBy { get; set; } // "newest", "helpful", "ratingHigh", "ratingLow"
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
