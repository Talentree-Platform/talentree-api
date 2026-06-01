using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Services
{
    /// <summary>
    /// High-level notification helper service
    /// Provides convenient methods for common notification scenarios
    /// Called by other services when events occur
    /// Handles message composition and bulk admin notifications
    /// </summary>
    public class NotificationHelperService : INotificationHelperService
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<NotificationHelperService> _logger;

        public NotificationHelperService(
            INotificationService notificationService,
            UserManager<AppUser> userManager,
            ILogger<NotificationHelperService> logger)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════
        // AUTHENTICATION & REGISTRATION
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyUserRegistered(string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Welcome to Talentree! 🎉",
                Message = "Your account has been created successfully. Please verify your email to complete registration.",
                ActionUrl = "/verify-email",
                ActionText = "Verify Email",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        public async Task NotifyEmailVerified(string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Email Verified ✅",
                Message = "Your email has been verified successfully. You can now access all features.",
                ActionUrl = "/dashboard",
                ActionText = "Go to Dashboard",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        public async Task NotifyPasswordResetSuccess(string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Password Changed ✅",
                Message = "Your password has been reset successfully. You can now login with your new password.",
                ActionUrl = "/login",
                ActionText = "Go to Login",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        // ═══════════════════════════════════════════════════════════
        // BUSINESS OWNER APPROVAL
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyBusinessOwnerApproved(string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Application Approved! 🎉",
                Message = "Congratulations! Your business owner application has been approved. You can now create and sell products.",
                ActionUrl = "/dashboard/products",
                ActionText = "Start Selling",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "BusinessOwnerProfile"
            });
        }

        public async Task NotifyBusinessOwnerRejected(string userId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Application Rejected",
                Message = $"Your business owner application was rejected.\n\nReason: {reason}\n\nYou can contact support for more information.",
                ActionUrl = "/support/tickets",
                ActionText = "Contact Support",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "BusinessOwnerProfile"
            });
        }

        // ═══════════════════════════════════════════════════════════
        // ACCOUNT MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyUserSuspended(string userId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Suspended ⚠️",
                Message = $"Your account has been temporarily suspended.\n\nReason: {reason}\n\nYou can appeal this decision by contacting support.",
                ActionUrl = "/support/tickets",
                ActionText = "Appeal",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        public async Task NotifyUserUnsuspended(string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Reactivated ✅",
                Message = "Your account suspension has been lifted. You can now access all features.",
                ActionUrl = "/dashboard",
                ActionText = "Go to Dashboard",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        public async Task NotifyUserBanned(string userId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Banned 🚫",
                Message = $"Your account has been permanently banned.\n\nReason: {reason}\n\nThis decision is final.",
                ActionUrl = "/support/tickets",
                ActionText = "Contact Support",
                Priority = NotificationPriority.Urgent,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        public async Task NotifyUserBlocked(string userId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Blocked 🔒",
                Message = $"Your account has been blocked due to suspicious activity.\n\nReason: {reason}\n\nPlease contact support.",
                ActionUrl = "/support/tickets",
                ActionText = "Get Help",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        public async Task NotifyUserUnblocked(string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Account,
                Title = "Account Unblocked ✅",
                Message = "Your account has been unblocked. You can now access your account normally.",
                ActionUrl = "/dashboard",
                ActionText = "Go to Dashboard",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        // ═══════════════════════════════════════════════════════════
        // PRODUCTS
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyProductApproved(string userId, int productId, string productName)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Product,
                Title = "Product Approved ✅",
                Message = $"Your product '{productName}' has been approved and is now visible in the marketplace.",
                ActionUrl = $"/dashboard/products/{productId}",
                ActionText = "View Product",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Product",
                RelatedEntityId = productId
            });
        }

        public async Task NotifyProductRejected(string userId, int productId, string productName, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Product,
                Title = "Product Rejected",
                Message = $"Your product '{productName}' was rejected.\n\nReason: {reason}\n\nPlease review and resubmit.",
                ActionUrl = $"/dashboard/products/{productId}/edit",
                ActionText = "Edit Product",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Product",
                RelatedEntityId = productId
            });
        }

        // ═══════════════════════════════════════════════════════════
        // SUPPORT TICKETS
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyTicketCreated(int ticketId, string ticketNumber)
        {
            // Notify admins
            await NotifyAllAdmins(
                "New Support Ticket",
                $"New support ticket {ticketNumber} awaiting assignment.",
                NotificationType.Support,
                $"/admin/support/tickets/{ticketId}"
            );
        }

        public async Task NotifyTicketStatusChanged(int ticketId, string ticketNumber, string userId, string newStatus)
        {
            var (title, message) = newStatus switch
            {
                "InProgress" => ("Ticket In Progress 👤", "Your ticket is now being worked on by our support team."),
                "AwaitingReply" => ("Your Response Needed 💬", "We need your response on this ticket. Please reply."),
                "Resolved" => ("Ticket Resolved ✅", "Your ticket has been marked as resolved."),
                "Closed" => ("Ticket Closed 📁", "Your ticket has been closed."),
                _ => ("Ticket Updated", $"Your ticket status has been updated to {newStatus}.")
            };

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Support,
                Title = title,
                Message = message,
                ActionUrl = $"/support/tickets/{ticketId}",
                ActionText = "View Ticket",
                Priority = newStatus == "AwaitingReply" ? NotificationPriority.High : NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "SupportTicket",
                RelatedEntityId = ticketId
            });
        }

        public async Task NotifyTicketAssigned(int ticketId, string ticketNumber, string adminId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = adminId,
                Type = NotificationType.Support,
                Title = "Ticket Assigned to You 📌",
                Message = $"Support ticket {ticketNumber} has been assigned to you.",
                ActionUrl = $"/admin/support/tickets/{ticketId}",
                ActionText = "Open Ticket",
                Priority = NotificationPriority.Normal,
                SendEmail = false,
                RelatedEntityType = "SupportTicket",
                RelatedEntityId = ticketId
            });
        }

        public async Task NotifyTicketMessageAdded(int ticketId, string ticketNumber, string adminId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = adminId,
                Type = NotificationType.Support,
                Title = "New Message on Ticket 💬",
                Message = $"There's a new message on ticket {ticketNumber}.",
                ActionUrl = $"/admin/support/tickets/{ticketId}",
                ActionText = "View Message",
                Priority = NotificationPriority.Normal,
                SendEmail = false,
                RelatedEntityType = "SupportTicket",
                RelatedEntityId = ticketId
            });
        }

        // ═══════════════════════════════════════════════════════════
        // COMPLAINTS
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyComplaintResolved(string userId, string resolution)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Complaint,
                Title = "Complaint Resolved ✅",
                Message = $"Your complaint has been resolved.\n\n{resolution}",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Complaint"
            });
        }

        public async Task NotifyComplaintRejected(string userId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Complaint,
                Title = "Complaint Rejected",
                Message = $"Your complaint was reviewed and could not be confirmed.\n\nReason: {reason}",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Complaint"
            });
        }

        public async Task NotifyAutoBlockApplied(string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.AutoBlock,
                Title = "Account Auto-Blocked 🔒",
                Message = "Your account has been automatically blocked due to multiple policy violations. Our team will review this within 24 hours.",
                ActionUrl = "/support/tickets",
                ActionText = "Appeal",
                Priority = NotificationPriority.Urgent,
                SendEmail = true,
                RelatedEntityType = "Account"
            });

            // Notify admins
            await NotifyAllAdmins(
                "Auto-Block Applied",
                $"User {userId} has been auto-blocked. Please review.",
                NotificationType.AutoBlock
            );
        }

        public async Task NotifyAutoBlockReviewed(string userId, string decision)
        {
            var (title, message) = decision switch
            {
                "Maintain" => ("Block Maintained 🔒", "Your account block has been reviewed and maintained. This decision is final."),
                "Warn" => ("Warning Issued ⚠️", "Your account block has been reviewed. You've been issued a warning. Avoid violations."),
                "Unblock" => ("Account Unblocked ✅", "Your account block has been reviewed and lifted. You can now access your account."),
                _ => ("Block Reviewed", $"Your account block has been reviewed. Decision: {decision}")
            };

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.AutoBlock,
                Title = title,
                Message = message,
                ActionUrl = decision == "Unblock" ? "/dashboard" : "/support/tickets",
                ActionText = decision == "Unblock" ? "Go to Dashboard" : "Contact Support",
                Priority = decision == "Unblock" ? NotificationPriority.Normal : NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Account"
            });
        }

        // ═══════════════════════════════════════════════════════════
        // ORDERS
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyOrderPlaced(string orderId, string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Order,
                Title = "Order Placed 📦",
                Message = $"Your order {orderId} has been created. Please proceed to payment to confirm.",
                ActionUrl = $"/orders/{orderId}",
                ActionText = "Complete Payment",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Order",
                RelatedEntityId = orderId.GetHashCode()
            });
        }

        public async Task NotifyPaymentSuccess(string orderId, string userId, decimal amount)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Payment,
                Title = "Payment Successful ✅",
                Message = $"Your payment of EGP {amount:F2} for order {orderId} has been confirmed. We're preparing your order.",
                ActionUrl = $"/orders/{orderId}",
                ActionText = "Track Order",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Order",
                RelatedEntityId = orderId.GetHashCode()
            });
        }

        public async Task NotifyPaymentFailed(string orderId, string userId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Payment,
                Title = "Payment Failed ❌",
                Message = $"Payment for order {orderId} failed.\n\nReason: {reason}\n\nPlease try again.",
                ActionUrl = $"/orders/{orderId}",
                ActionText = "Retry Payment",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Order",
                RelatedEntityId = orderId.GetHashCode()
            });
        }

        // ═══════════════════════════════════════════════════════════
        // PRODUCTION REQUESTS
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyProductionRequestCreated(int requestId, string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.ProductionRequest,
                Title = "Production Request Submitted 📝",
                Message = "Your production request has been submitted. Our team will review it and send you a quote shortly.",
                ActionUrl = $"/production-requests/{requestId}",
                ActionText = "View Request",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "ProductionRequest",
                RelatedEntityId = requestId
            });

            // Notify admins
            await NotifyAllAdmins(
                "New Production Request",
                $"Production request {requestId} needs review.",
                NotificationType.ProductionRequest,
                $"/admin/production-requests/{requestId}"
            );
        }

        public async Task NotifyProductionQuoteSent(int requestId, string userId, decimal amount)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.ProductionRequest,
                Title = "Quote Ready 💰",
                Message = $"Your production quote is ready: EGP {amount:F2}\n\nPlease review and proceed with payment.",
                ActionUrl = $"/production-requests/{requestId}",
                ActionText = "View & Pay",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "ProductionRequest",
                RelatedEntityId = requestId
            });
        }

        public async Task NotifyProductionStarted(int requestId, string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.ProductionRequest,
                Title = "Production Started 🏭",
                Message = "Your production has started. You'll receive updates as it progresses.",
                ActionUrl = $"/production-requests/{requestId}",
                ActionText = "Track Progress",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "ProductionRequest",
                RelatedEntityId = requestId
            });
        }

        public async Task NotifyProductionCompleted(int requestId, string userId)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.ProductionRequest,
                Title = "Production Completed ✅",
                Message = "Your production has been completed. Please arrange delivery or pickup.",
                ActionUrl = $"/production-requests/{requestId}",
                ActionText = "Arrange Delivery",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "ProductionRequest",
                RelatedEntityId = requestId
            });
        }

        public async Task NotifyProductionRejected(int requestId, string userId, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.ProductionRequest,
                Title = "Request Rejected",
                Message = $"Your production request was rejected.\n\nReason: {reason}",
                ActionUrl = $"/production-requests/{requestId}",
                ActionText = "View Details",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "ProductionRequest",
                RelatedEntityId = requestId
            });
        }

        // ═══════════════════════════════════════════════════════════
        // PAYOUTS
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyPayoutRequested(string userId, decimal amount)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Payout,
                Title = "Payout Requested 💸",
                Message = $"Your payout request for EGP {amount:F2} has been submitted. We'll process it within 3-5 business days.",
                ActionUrl = "/payouts",
                ActionText = "View Payouts",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Payout"
            });
        }

        public async Task NotifyPayoutApproved(string userId, decimal amount)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Payout,
                Title = "Payout Approved ✅",
                Message = $"Your payout of EGP {amount:F2} has been approved. It will be transferred within 1-2 business days.",
                ActionUrl = "/payouts",
                ActionText = "View Status",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Payout"
            });
        }

        public async Task NotifyPayoutCompleted(string userId, decimal amount)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Payout,
                Title = "Payout Completed 💰",
                Message = $"Your payout of EGP {amount:F2} has been completed and transferred to your bank account.",
                ActionUrl = "/payouts",
                ActionText = "View Details",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Payout"
            });
        }

        public async Task NotifyPayoutRejected(string userId, decimal amount, string reason)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.Payout,
                Title = "Payout Rejected ❌",
                Message = $"Your payout request for EGP {amount:F2} was rejected.\n\nReason: {reason}\n\nPlease contact support.",
                ActionUrl = "/support/tickets",
                ActionText = "Contact Support",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Payout"
            });
        }

        // ═══════════════════════════════════════════════════════════
        // BULK NOTIFICATIONS
        // ═══════════════════════════════════════════════════════════

        public async Task NotifyAllAdmins(
            string title,
            string message,
            NotificationType type,
            string? actionUrl = null)
        {
            try
            {
                // Get all admin users
                var adminIds = await GetAllAdminIdsAsync();

                if (!adminIds.Any())
                {
                    _logger.LogWarning("No admins found to notify");
                    return;
                }

                // Send bulk notification
                await _notificationService.CreateBulkNotificationAsync(
                    adminIds,
                    type,
                    title,
                    message,
                    actionUrl,
                    "View",
                    NotificationPriority.High
                );

                _logger.LogInformation("Notified {Count} admins: {Title}", adminIds.Count, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying all admins");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════

        private async Task<List<string>> GetAllAdminIdsAsync()
        {
            try
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                return admins.Select(a => a.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin IDs");
                return new List<string>();
            }
        }
    }
}