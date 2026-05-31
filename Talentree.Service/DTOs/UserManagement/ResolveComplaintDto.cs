using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.UserManagement
{
    public class ResolveComplaintDto
    {
        [Required]
        public int ComplaintId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Resolution { get; set; } = null!;

        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        public bool BlockUser { get; set; } = false;
    }
}