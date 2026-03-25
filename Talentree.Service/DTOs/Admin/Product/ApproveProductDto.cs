using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs.Admin.Product
{
    public class ApproveProductDto
    {
        [Required]
        public int ProductId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
