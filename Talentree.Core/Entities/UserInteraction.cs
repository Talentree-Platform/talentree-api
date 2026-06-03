
using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// Records user interactions for AI recommendation system
    /// Tracks: views, clicks, purchases, reorders
    /// Used by AI team for personalization and recommendations
    /// </summary>
    public class UserInteraction : BaseEntity
    {
        // User Info
        public string UserId { get; set; }  // FK to AppUser
        public UserInteractionType UserType { get; set; }  // Customer, Owner, Supplier

        // Item Info
        public int ItemId { get; set; }
        public UserInteractionItemType ItemType { get; set; }  // Product, RawMaterial

        // Interaction Details
        public UserInteractionActionType ActionType { get; set; }  // View, Click, Purchase, Reorder

        // Context Data
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Timestamps
        public DateTime InteractionTimestamp { get; set; }  // When user performed action
        public DateTime CreatedAt { get; set; }  // When record was logged

        // Navigation
        public AppUser User { get; set; }
    }


}