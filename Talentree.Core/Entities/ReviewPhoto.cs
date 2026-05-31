namespace Talentree.Core.Entities
{
    public class ReviewPhoto : BaseEntity
    {
        public int ReviewId { get; set; }
        public ProductReview Review { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
