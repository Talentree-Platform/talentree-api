namespace Talentree.Service.DTOs.Reviews
{
    public class ReviewAnalyticsDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalResponded { get; set; }
        public double ResponseRate { get; set; } // percentage
        public RatingDistributionDto Distribution { get; set; } = new();
        public List<SentimentTrendDto> SentimentTrend { get; set; } = new();
    }

    public class RatingDistributionDto
    {
        public int FiveStar { get; set; }
        public int FourStar { get; set; }
        public int ThreeStar { get; set; }
        public int TwoStar { get; set; }
        public int OneStar { get; set; }
    }

    public class SentimentTrendDto
    {
        public string Month { get; set; } = string.Empty; // e.g. "2026-04"
        public double AverageSentiment { get; set; }
        public int ReviewCount { get; set; }
    }
}