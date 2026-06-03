namespace Talentree.Service.DTOs.Admin.Supplier
{
    public class SupplierPerformanceDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double? AverageFulfillmentTimeHours { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public double IssueRatePercentage { get; set; }
    }
}
