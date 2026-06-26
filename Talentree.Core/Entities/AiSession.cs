using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class AiSession : AuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign Key to Business Owner
        public int BusinessOwnerProfileId { get; set; }
        public BusinessOwnerProfile BusinessOwnerProfile { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
    }
}
