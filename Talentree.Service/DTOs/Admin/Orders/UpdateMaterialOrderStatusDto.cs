using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Orders
{
    public class UpdateMaterialOrderStatusDto
    {
        [Required]
        public MaterialOrderStatus NewStatus { get; set; }

        public string? Reason { get; set; }
    }
}
