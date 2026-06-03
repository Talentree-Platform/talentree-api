using System;
using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Orders
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public CustomerOrderStatus NewStatus { get; set; }
        
        [Required]
        public string Reason { get; set; } = null!;
        
        public string? TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}
