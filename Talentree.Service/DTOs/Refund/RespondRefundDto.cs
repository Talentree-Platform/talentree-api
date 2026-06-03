using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Refund
{
    public class RespondRefundDto
    {
        [Required]
        public string Response { get; set; } = null!; // "Accepted" or "Rejected"
        
        public string? Notes { get; set; }
    }
}
