namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class CustomerProductFilterParams
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } // "priceAsc", "priceDesc", "rating", "newest", "featured"
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
