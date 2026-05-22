using System;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    public class OrderStatusHistory : BaseEntity
    {
        public int OrderId { get; set; }
        public CustomerOrder Order { get; set; } = null!;
        public CustomerOrderStatus Status { get; set; }
        public string? Notes { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
