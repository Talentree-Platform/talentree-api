using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class ProductReview : BaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string CustomerUserId { get; set; } = null!;
        public AppUser Customer { get; set; } = null!;

        public string CustomerName { get; set; } = null!;
        public byte Rating { get; set; } // 1-5
        public string? ReviewText { get; set; }
        public bool IsAnonymous { get; set; } = false;

        // Owner response
        public string? OwnerResponse { get; set; }
        public DateTime? ResponseAt { get; set; }

        // AI fields
        public float? SentimentScore { get; set; }   // -1 to +1
        public string? SentimentLabel { get; set; }  // Positive/Neutral/Negative
        public bool FlaggedToxic { get; set; } = false;
    }
}