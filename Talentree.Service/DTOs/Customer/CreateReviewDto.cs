using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Customer
{
    public class CreateReviewDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public byte Rating { get; set; }

        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string? ReviewTitle { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Review text must be at least 10 characters.")]
        public string ReviewText { get; set; } = null!;

        public bool IsAnonymous { get; set; } = false;
    }
}
