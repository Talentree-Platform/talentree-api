namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Filter + pagination parameters for the FR-AD-11 low-stock list.
    /// </summary>
    public class LowStockFilterDto
    {
        public int? CategoryId { get; set; }
        public int? BusinessOwnerProfileId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
