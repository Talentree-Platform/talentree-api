using System;

namespace Talentree.Service.Messaging.Contracts
{
    public abstract class BaseMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
