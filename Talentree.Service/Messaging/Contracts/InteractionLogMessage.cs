using Talentree.Core.Enums;

namespace Talentree.Service.Messaging.Contracts
{
    public class InteractionLogMessage : BaseMessage
    {
        public string UserId { get; set; } = string.Empty;
        public UserInteractionType UserType { get; set; }
        public int ItemId { get; set; }
        public UserInteractionItemType ItemType { get; set; }
        public UserInteractionActionType ActionType { get; set; }
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
