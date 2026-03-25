using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Products
{
    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public ProductStatus? Status { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;

        private int _pageIndex = 1;
        private int _pageSize = 20;

        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
        }
    }
}
