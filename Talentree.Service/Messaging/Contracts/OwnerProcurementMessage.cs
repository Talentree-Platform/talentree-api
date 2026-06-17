namespace Talentree.Service.Messaging.Contracts
{
    public class OwnerProcurementMessage : BaseMessage
    {
        public string UserId { get; set; } = string.Empty;
        public int TopK { get; set; } = 6;
    }
}
