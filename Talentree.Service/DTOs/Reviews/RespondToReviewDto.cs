using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Reviews
{
    public class RespondToReviewDto
    {
        [Required]
        [MaxLength(500, ErrorMessage = "Response cannot exceed 500 characters")]
        public string Response { get; set; } = string.Empty;
    }
}