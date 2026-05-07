using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class ChatHistory : BaseEntity
    {
        public int BusinessOwnerProfileId { get; set; }
        public BusinessOwnerProfile BusinessOwnerProfile { get; set; } = null!;

        public string Role { get; set; } = null!; // User / Assistant
        public string Message { get; set; } = null!;
        public string? Agent { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}