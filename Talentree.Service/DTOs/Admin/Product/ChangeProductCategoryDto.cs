using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Admin.Product
{
    /// <summary>
    /// Payload for FR-AD-10 re-categorize (change category) action.
    /// </summary>
    public class ChangeProductCategoryDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int NewCategoryId { get; set; }
    }
}
