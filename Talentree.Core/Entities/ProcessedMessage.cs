using System;

namespace Talentree.Core.Entities
{
    public class ProcessedMessage
    {
        public Guid MessageId { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
