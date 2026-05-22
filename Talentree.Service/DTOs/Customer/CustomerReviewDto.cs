using System;
using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class CustomerReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? ReviewTitle { get; set; }
        public byte Rating { get; set; }
        public string? ReviewText { get; set; }
        public bool IsAnonymous { get; set; }
        public int HelpfulVotes { get; set; }
        public string? OwnerResponse { get; set; }
        public DateTime? ResponseAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
    }
}
