using System;

namespace Talentree.Service.DTOs.Customer
{
    public class OrderStatusHistoryDto
    {
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
