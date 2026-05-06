using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.UserManagement
{
    public class BanUserDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Reason { get; set; } = null!;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public bool IsPermanent { get; set; } = true;
         
    }
}