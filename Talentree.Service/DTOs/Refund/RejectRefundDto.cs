using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Refund
{
    public class RejectRefundDto
    {
        [Required]
        public string Reason { get; set; } = null!;
    }
}
