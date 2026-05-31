using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class CustomerProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public float? AvgRating { get; set; }
        public string? Tags { get; set; }
        public string CategoryName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string BrandName { get; set; } = null!;
        public int BrandId { get; set; }
        public string? BrandLogoUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<CustomerProductDto> SimilarProducts { get; set; } = new();
    }
}
