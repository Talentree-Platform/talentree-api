using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class ProductFilterParams
    {
        public string? Search { get; set; }
        public ProductStatus? Status { get; set; }
        public string? SortBy { get; set; } // "date", "price", "stock"
        public bool SortDescending { get; set; } = true;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
