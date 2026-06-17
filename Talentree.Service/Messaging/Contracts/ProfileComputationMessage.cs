namespace Talentree.Service.Messaging.Contracts
{
    public class ProfileComputationMessage : BaseMessage
    {
        public string UserId { get; set; } = string.Empty;
    }
}
