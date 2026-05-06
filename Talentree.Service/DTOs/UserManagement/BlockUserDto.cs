using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.UserManagement
{
    public class BlockUserDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Reason { get; set; } = null!;
    }
}