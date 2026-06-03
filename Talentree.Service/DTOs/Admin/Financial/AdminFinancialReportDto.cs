using System;
using System.Collections.Generic;

namespace Talentree.Service.DTOs.Admin.Financial
{
    public class AdminFinancialReportDto
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        
        public decimal TotalRevenue { get; set; }
        public decimal TotalPayouts { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal NetPlatformRevenue { get; set; }
        
        public List<TopSellerDto> TopSellersByRevenue { get; set; } = new();
        public List<DailyRevenueDto> DailyRevenueSeries { get; set; } = new();
    }

    public class TopSellerDto
    {
        public string BusinessOwnerId { get; set; } = null!;
        public string BusinessName { get; set; } = null!;
        public decimal TotalRevenue { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }
}
