using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Refund
{
    public class CreateRefundRequestDto
    {
        [Required]
        public string Reason { get; set; } = null!;
    }
}
