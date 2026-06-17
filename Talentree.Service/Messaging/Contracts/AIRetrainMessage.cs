namespace Talentree.Service.Messaging.Contracts
{
    public class AIRetrainMessage : BaseMessage
    {
        public string ModelType { get; set; } = string.Empty; // "customer" or "owner"
    }
}
