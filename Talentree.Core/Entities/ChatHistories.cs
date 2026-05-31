using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    public class ChatHistory : BaseEntity
    {
        public int BusinessOwnerProfileId { get; set; }
        public BusinessOwnerProfile BusinessOwnerProfile { get; set; } = null!;

        public string Role { get; set; } = null!; // User / Assistant
        public string Message { get; set; } = null!;
        public string? Agent { get; set; }

        public AiAgentType AiAgentType { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}