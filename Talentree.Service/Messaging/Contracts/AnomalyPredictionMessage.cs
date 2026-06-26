namespace Talentree.Service.Messaging.Contracts
{
    public class AnomalyPredictionMessage : BaseMessage
    {
        public int TransactionId { get; set; }
    }
}
