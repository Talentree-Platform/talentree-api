using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Support
{
    public class UpdateTicketStatusDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public TicketStatus Status { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }
}