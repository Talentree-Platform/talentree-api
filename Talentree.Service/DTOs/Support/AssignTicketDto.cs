using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Support
{
    public class AssignTicketDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public string AdminId { get; set; } = null!;
    }
}