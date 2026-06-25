namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Per-product analytics response for FR-AD-10.
    /// </summary>
    public class AdminProductAnalyticsDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public long TotalViews { get; set; }
        public long CartAddCount { get; set; }
        public long PurchaseCount { get; set; }

        /// <summary>PurchaseCount / max(TotalViews, 1) * 100 — percentage.</summary>
        public double ConversionRate { get; set; }

        public decimal RevenueGenerated { get; set; }
        public float? AvgRating { get; set; }
    }
}
