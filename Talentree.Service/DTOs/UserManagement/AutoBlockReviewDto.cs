using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.UserManagement
{
    public class AutoBlockReviewDto
    {
        [Required]
        public int AutoBlockLogId { get; set; }

        [Required]
        [StringLength(100)]
        public string Decision { get; set; } = null!; // "Maintain", "Warn", "Unblock"

        [StringLength(1000)]
        public string? AdminNotes { get; set; }
    }
}