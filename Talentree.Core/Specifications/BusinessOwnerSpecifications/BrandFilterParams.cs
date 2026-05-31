namespace Talentree.Core.Specifications.BusinessOwnerSpecifications
{
    public class BrandFilterParams
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? SortBy { get; set; } // "name", "newest"
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
