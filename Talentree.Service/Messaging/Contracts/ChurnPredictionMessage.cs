namespace Talentree.Service.Messaging.Contracts
{
    public class ChurnPredictionMessage : BaseMessage
    {
        public string UserId { get; set; } = string.Empty;
    }
}
