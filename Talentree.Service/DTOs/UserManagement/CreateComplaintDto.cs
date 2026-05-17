using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.UserManagement
{
    public class CreateComplaintDto
    {
        [Required]
        public string ReportedUserId { get; set; } = null!;

        [Required]
        public ViolationType ViolationType { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 20)]
        public string Description { get; set; } = null!;

        public string? RelatedOrderId { get; set; }
        public string? RelatedProductId { get; set; }

        [StringLength(100)]
        public string? RelatedContext { get; set; } // e.g., "MaterialOrder", "Product"

    }
}