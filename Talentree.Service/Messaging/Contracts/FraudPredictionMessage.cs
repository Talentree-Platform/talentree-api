namespace Talentree.Service.Messaging.Contracts
{
    public class FraudPredictionMessage : BaseMessage
    {
        public int RequestId { get; set; }
    }
}
