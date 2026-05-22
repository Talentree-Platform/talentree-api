using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class BrandDetailDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = null!;
        public string BusinessDescription { get; set; } = null!;
        public string BusinessCategory { get; set; } = null!;
        public string? BusinessAddress { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WebsiteLink { get; set; }
        public string? BusinessLogoUrl { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public List<CustomerProductDto> Products { get; set; } = new();
    }
}
