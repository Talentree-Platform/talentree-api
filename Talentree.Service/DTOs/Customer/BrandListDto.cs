namespace Talentree.Service.DTOs.Customer
{
    public class BrandListDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = null!;
        public string BusinessDescription { get; set; } = null!;
        public string BusinessCategory { get; set; } = null!;
        public string? BusinessLogoUrl { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public int ProductCount { get; set; }
    }
}
