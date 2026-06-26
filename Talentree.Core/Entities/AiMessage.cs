using System;
using System.Collections.Generic;

namespace Talentree.Core.Entities
{
    public class AiMessage : BaseEntity
    {
        public Guid SessionId { get; set; }
        public AiSession Session { get; set; } = null!;

        public string Role { get; set; } = string.Empty; // "user", "assistant", "system"
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property for generated media
        public ICollection<AiImage> GeneratedImages { get; set; } = new List<AiImage>();
    }
}
