namespace Talentree.Service.Messaging.Contracts
{
    public class TriagePredictionMessage : BaseMessage
    {
        public int TicketId { get; set; }
    }
}
