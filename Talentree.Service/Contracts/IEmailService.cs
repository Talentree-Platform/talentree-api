
using Talentree.Core.Entities.Identity;

namespace Talentree.Service.Contracts
{
 

    public interface IEmailService
    {
        /// <summary>
        /// Send email to a single recipient
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Send email to multiple recipients
        /// </summary>
        Task SendBulkEmailAsync(List<string> toEmails, string subject, string body, bool isHtml = true);

        /// <summary>
        /// Send email with file attachment
        /// </summary>
        Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            string attachmentPath,
            bool isHtml = true);

        /// <summary>
        /// Send OTP (One-Time Password) email
        /// </summary>
        Task SendOtpAsync(string toEmail, string otpCode, OtpPurpose purpose);

        /// <summary>
        /// Send welcome email
        /// </summary>
        Task SendWelcomeEmailAsync(string toEmail, string userName);

        /// <summary>
        /// Send account approval email
        /// </summary>
        Task SendApprovalEmailAsync(string toEmail, string userName);

        /// <summary>
        /// Send account rejection email
        /// </summary>
        Task SendRejectionEmailAsync(string toEmail, string userName, string reason);

        /// <summary>
        /// Send password reset email
        /// </summary>
        Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);

        /// <summary>
        /// Send order confirmation email
        /// </summary>
        Task SendOrderConfirmationEmailAsync(string toEmail, string userName, string orderNumber, decimal total);

        /// <summary>
        /// Send support ticket created email
        /// </summary>
        Task SendTicketCreatedEmailAsync(string toEmail, string userName, string ticketNumber);
    }
}