using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Products
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Tags { get; set; }
        public ProductStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int BusinessOwnerProfileId { get; set; }
        public string? MainImageUrl { get; set; }
        public List<ProductImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
    }
}
