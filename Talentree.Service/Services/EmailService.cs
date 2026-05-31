using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Settings;
using Talentree.Service.Contracts;
using Talentree.Service.Templates;

namespace Talentree.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.From, _emailSettings.DisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml,
                    Priority = MailPriority.Normal
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        _emailSettings.Username,
                        _emailSettings.Password
                    ),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}: {Message}", toEmail, ex.Message);
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendBulkEmailAsync(List<string> toEmails, string subject, string body, bool isHtml = true)
        {
            var tasks = toEmails.Select(email => SendEmailAsync(email, subject, body, isHtml));
            await Task.WhenAll(tasks);
        }

        public async Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            string attachmentPath,
            bool isHtml = true)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.From, _emailSettings.DisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                mailMessage.To.Add(toEmail);

                if (File.Exists(attachmentPath))
                {
                    var attachment = new Attachment(attachmentPath);
                    mailMessage.Attachments.Add(attachment);
                }

                using var smtpClient = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        _emailSettings.Username,
                        _emailSettings.Password
                    )
                };

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email with attachment sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email with attachment to {Email}", toEmail);
                throw;
            }
        }

        // ✅ NEW: Send OTP Email
        public async Task SendOtpAsync(string toEmail, string otpCode, OtpPurpose purpose)
        {
            string subject;
            string body;

            switch (purpose)
            {
                case OtpPurpose.EmailVerification:
                    subject = "Verify Your Email - Talentree";
                    body = EmailTemplates.GetEmailVerificationTemplate(otpCode);
                    break;

                case OtpPurpose.ResetPassword:
                    subject = "Reset Your Password - Talentree";
                    body = EmailTemplates.GetPasswordResetOtpTemplate(otpCode);
                    break;

                case OtpPurpose.TwoFactorAuth:
                    subject = "Your Two-Factor Authentication Code - Talentree";
                    body = EmailTemplates.GetTwoFactorAuthTemplate(otpCode);
                    break;

                default:
                    subject = "Your Verification Code - Talentree";
                    body = EmailTemplates.GetGenericOtpTemplate(otpCode);
                    break;
            }

            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        // ✅ NEW: Send Welcome Email
        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to Talentree! 🎉";
            var body = EmailTemplates.GetWelcomeEmail(userName);
            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        // ✅ NEW: Send Approval Email
        public async Task SendApprovalEmailAsync(string toEmail, string userName)
        {
            var subject = "Your Account Has Been Approved! ✅";
            var body = EmailTemplates.GetApprovalEmail(userName);
            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        // ✅ NEW: Send Rejection Email
        public async Task SendRejectionEmailAsync(string toEmail, string userName, string reason)
        {
            var subject = "Application Status Update - Talentree";
            var body = EmailTemplates.GetRejectionEmail(userName, reason);
            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        // ✅ NEW: Send Password Reset Email
        public async Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink)
        {
            var subject = "Reset Your Password - Talentree";
            var body = EmailTemplates.GetPasswordResetEmail(userName, resetLink);
            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        // ✅ NEW: Send Order Confirmation Email
        public async Task SendOrderConfirmationEmailAsync(string toEmail, string userName, string orderNumber, decimal total)
        {
            var subject = $"Order Confirmation - {orderNumber}";
            var body = EmailTemplates.GetOrderConfirmationEmail(userName, orderNumber, total);
            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }

        // ✅ NEW: Send Ticket Created Email
        public async Task SendTicketCreatedEmailAsync(string toEmail, string userName, string ticketNumber)
        {
            var subject = $"Support Ticket Created - {ticketNumber}";
            var body = EmailTemplates.GetTicketCreatedEmail(userName, ticketNumber);
            await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
    }
}