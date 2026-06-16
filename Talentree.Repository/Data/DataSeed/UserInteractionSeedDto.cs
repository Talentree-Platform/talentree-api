using System;

namespace Talentree.Repository.Data.DataSeed
{
    public class UserInteractionSeedDto
    {
        public int UserId { get; set; }
        public int UserType { get; set; }
        public int ItemId { get; set; }
        public int ItemType { get; set; }
        public int ActionType { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string InteractionTimestamp { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
