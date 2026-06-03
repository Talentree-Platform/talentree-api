using System;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Orders
{
    public class AdminOrderFilterDto
    {
        public string? Search { get; set; }
        public CustomerOrderStatus? Status { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortBy { get; set; } // e.g. "date", "amount"
        public bool SortDesc { get; set; } = true;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
