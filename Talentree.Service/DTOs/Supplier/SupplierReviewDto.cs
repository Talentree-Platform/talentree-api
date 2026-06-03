namespace Talentree.Service.DTOs.Supplier
{
    public class SupplierReviewDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string BusinessOwnerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
