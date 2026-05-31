using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.Auth
{
    public class ResendVerificationEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}