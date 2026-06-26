using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.UserManagement
{
    public class CreateComplaintDto : IValidatableObject
    {
        public string? ReportedUserId { get; set; }

        [Required]
        public ViolationType ViolationType { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 20)]
        public string Description { get; set; } = null!;

        public string? RelatedOrderId { get; set; }
        public string? RelatedProductId { get; set; }
        public string? RelatedBrandId { get; set; }

        [StringLength(100)]
        public string? RelatedContext { get; set; } // e.g., "MaterialOrder", "Product"

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(ReportedUserId) &&
                string.IsNullOrEmpty(RelatedProductId) &&
                string.IsNullOrEmpty(RelatedBrandId) &&
                string.IsNullOrEmpty(RelatedOrderId))
            {
                yield return new ValidationResult(
                    "At least one target identifier (ReportedUserId, RelatedProductId, RelatedBrandId, or RelatedOrderId) must be provided.",
                    new[] { nameof(ReportedUserId), nameof(RelatedProductId), nameof(RelatedBrandId), nameof(RelatedOrderId) }
                );
            }
        }
    }
}