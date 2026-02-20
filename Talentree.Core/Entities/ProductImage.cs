using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        // FK
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
