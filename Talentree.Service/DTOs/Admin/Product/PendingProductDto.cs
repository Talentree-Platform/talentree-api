
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Product
{
   
    public class PendingProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Tags { get; set; }

        // Category
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Business Owner
        public int BusinessOwnerProfileId { get; set; }
        public string BusinessOwnerName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;

        // Images
        public string? MainImageUrl { get; set; }
        public int ImageCount { get; set; }

        // Status
        public ProductStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}