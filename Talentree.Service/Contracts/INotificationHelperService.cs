using Talentree.Core.Enums;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// High-level notification helper service
    /// Provides simple methods to send notifications for specific events
    /// Called by other services when events occur
    /// Handles message composition and notification type selection
    /// </summary>
    public interface INotificationHelperService
    {
        // ═══════════════════════════════════════════════════════════
        // AUTHENTICATION & REGISTRATION
        // ═══════════════════════════════════════════════════════════

        Task NotifyUserRegistered(string userId);
        Task NotifyEmailVerified(string userId);
        Task NotifyPasswordResetSuccess(string userId);

        // ═══════════════════════════════════════════════════════════
        // BUSINESS OWNER APPROVAL
        // ═══════════════════════════════════════════════════════════

        Task NotifyBusinessOwnerApproved(string userId);
        Task NotifyBusinessOwnerRejected(string userId, string reason);

        // ═══════════════════════════════════════════════════════════
        // ACCOUNT MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        Task NotifyUserSuspended(string userId, string reason);
        Task NotifyUserUnsuspended(string userId);
        Task NotifyUserBanned(string userId, string reason);
        Task NotifyUserBlocked(string userId, string reason);
        Task NotifyUserUnblocked(string userId);

        // ═══════════════════════════════════════════════════════════
        // PRODUCTS
        // ═══════════════════════════════════════════════════════════

        Task NotifyProductApproved(string userId, int productId, string productName);
        Task NotifyProductRejected(string userId, int productId, string productName, string reason);

        // ═══════════════════════════════════════════════════════════
        // SUPPORT TICKETS
        // ═══════════════════════════════════════════════════════════

        Task NotifyTicketCreated(int ticketId, string ticketNumber);
        Task NotifyTicketStatusChanged(int ticketId, string ticketNumber, string userId, string newStatus);
        Task NotifyTicketAssigned(int ticketId, string ticketNumber, string adminId);
        Task NotifyTicketMessageAdded(int ticketId, string ticketNumber, string adminId);

        // ═══════════════════════════════════════════════════════════
        // COMPLAINTS
        // ═══════════════════════════════════════════════════════════

        Task NotifyComplaintResolved(string userId, string resolution);
        Task NotifyComplaintRejected(string userId, string reason);
        Task NotifyAutoBlockApplied(string userId);
        Task NotifyAutoBlockReviewed(string userId, string decision);

        // ═══════════════════════════════════════════════════════════
        // ORDERS
        // ═══════════════════════════════════════════════════════════

        Task NotifyOrderPlaced(string orderId, string userId);
        Task NotifyPaymentSuccess(string orderId, string userId, decimal amount);
        Task NotifyPaymentFailed(string orderId, string userId, string reason);

        // ═══════════════════════════════════════════════════════════
        // PRODUCTION REQUESTS
        // ═══════════════════════════════════════════════════════════

        Task NotifyProductionRequestCreated(int requestId, string userId);
        Task NotifyProductionQuoteSent(int requestId, string userId, decimal amount);
        Task NotifyProductionStarted(int requestId, string userId);
        Task NotifyProductionCompleted(int requestId, string userId);
        Task NotifyProductionRejected(int requestId, string userId, string reason);

        // ═══════════════════════════════════════════════════════════
        // PAYOUTS
        // ═══════════════════════════════════════════════════════════

        Task NotifyPayoutRequested(string userId, decimal amount);
        Task NotifyPayoutApproved(string userId, decimal amount);
        Task NotifyPayoutCompleted(string userId, decimal amount);
        Task NotifyPayoutRejected(string userId, decimal amount, string reason);

        // ═══════════════════════════════════════════════════════════
        // BULK NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Send notification to all admin users
        /// </summary>
        Task NotifyAllAdmins(string title, string message, NotificationType type, string? actionUrl = null);
    }
}