namespace Talentree.Service.DTOs.Reviews
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty; // "Anonymous" if IsAnonymous
        public byte Rating { get; set; }
        public string? ReviewText { get; set; }
        public bool IsAnonymous { get; set; }
        public bool HasResponse { get; set; }
        public string? OwnerResponse { get; set; }
        public DateTime? ResponseAt { get; set; }
        public bool CanEditResponse { get; set; } // true if within 24h of posting response
        public float? SentimentScore { get; set; }
        public string? SentimentLabel { get; set; }
        public bool FlaggedToxic { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}