using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs.Admin.Product
{
    public class RejectProductDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10,
            ErrorMessage = "Rejection reason must be between 10 and 1000 characters")]
        public string Reason { get; set; } = null!;
    }
}
