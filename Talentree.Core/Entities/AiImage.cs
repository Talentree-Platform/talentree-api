using System;

namespace Talentree.Core.Entities
{
    public class AiImage : BaseEntity
    {
        // Foreign Key pointing directly to the generating message
        public int MessageId { get; set; }
        public AiMessage Message { get; set; } = null!;

        public string StorageUrl { get; set; } = string.Empty;
        public string R2ObjectKey { get; set; } = string.Empty;
        
        // Storing the enhanced prompt used for generation
        public string EnhancedPrompt { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
