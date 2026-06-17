namespace Talentree.Service.Messaging.Contracts
{
    public class SentimentPredictionMessage : BaseMessage
    {
        public int ReviewId { get; set; }
    }
}
