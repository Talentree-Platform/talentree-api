using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class HomepageDto
    {
        public List<CategoryDto> Categories { get; set; } = new();
        public List<CustomerProductDto> FeaturedProducts { get; set; } = new();
        public List<CustomerProductDto> TrendingProducts { get; set; } = new();
    }
}
