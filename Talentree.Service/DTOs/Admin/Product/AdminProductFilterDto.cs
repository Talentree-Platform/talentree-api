using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Filter + pagination parameters for FR-AD-10 all-products admin view.
    /// </summary>
    public class AdminProductFilterDto
    {
        public ProductStatus? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? BusinessOwnerProfileId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        /// <summary>Filter by product name (case-insensitive partial match).</summary>
        public string? Search { get; set; }

        /// <summary>Sort field: "name" | "price" | "createdAt" | "views" | "stock". Default: "createdAt".</summary>
        public string SortBy { get; set; } = "createdAt";

        public bool SortDesc { get; set; } = true;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
