using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class ProductRatingDistributionDto
    {
        public int TotalReviews { get; set; }
        public float AverageRating { get; set; }
        public Dictionary<int, int> StarCounts { get; set; } = new()
        {
            { 5, 0 },
            { 4, 0 },
            { 3, 0 },
            { 2, 0 },
            { 1, 0 }
        };
    }
}
