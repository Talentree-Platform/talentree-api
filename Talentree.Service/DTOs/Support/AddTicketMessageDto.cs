using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Talentree.Service.DTOs.Support
{
    public class AddTicketMessageDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 5)]
        public string Content { get; set; } = null!;

        public List<IFormFile>? Attachments { get; set; }
    }
}