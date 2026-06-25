using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Payload for FR-AD-09 bulk approve / bulk reject actions.
    /// At least one ProductId must be provided.
    /// Reason is required for bulk reject.
    /// </summary>
    public class BulkProductActionDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one ProductId is required.")]
        public List<int> ProductIds { get; set; } = new();

        /// <summary>Required when used for bulk-reject or request-changes.</summary>
        [StringLength(1000)]
        public string? Reason { get; set; }
    }
}
