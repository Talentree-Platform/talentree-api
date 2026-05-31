using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Support
{
    public class CreateTicketDto
    {
        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string Subject { get; set; } = null!;

        [Required]
        [StringLength(5000, MinimumLength = 20)]
        public string Description { get; set; } = null!;

        [Required]
        public TicketCategory Category { get; set; }

        public List<IFormFile>? Attachments { get; set; }
    }
}