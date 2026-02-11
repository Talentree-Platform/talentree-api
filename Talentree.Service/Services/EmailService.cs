// Talentree.Service/Services/EmailService.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using Talentree.Core.Entities.Identity;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Email sending service implementation
    /// Sends OTPs, verification emails, and notifications
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        /// <summary>
        /// Sends OTP code for email verification or password reset
        /// </summary>
        public async Task SendOtpAsync(string email, string otpCode, OtpPurpose purpose)
        {
            string subject;
            string body;

            // Determine which template to use based on purpose
            switch (purpose)
            {
                case OtpPurpose.VerifyEmail:
                    subject = "Verify Your Email - Talentree Platform";
                    body = GetEmailVerificationTemplate(otpCode);
                    break;

                case OtpPurpose.ResetPassword:
                    subject = "Reset Your Password - Talentree Platform";
                    body = GetPasswordResetTemplate(otpCode);
                    break;

                default:
                    throw new ArgumentException($"Unknown OTP purpose: {purpose}");
            }

            await SendEmailAsync(email, subject, body);
        }

        /// <summary>
        /// Sends an email asynchronously using SMTP configuration from appsettings or secrets.
        /// 
        /// This method handles:
        /// - Safe parsing of the SMTP port with a default fallback (587)
        /// - Required SMTP credentials (host, user, password)
        /// - Optional sender email and name with defaults
        /// - HTML-formatted emails with high priority
        /// - Logging for both successful sends and exceptions
        /// 
        /// Throws InvalidOperationException if required configuration is missing or
        /// if sending the email fails due to SMTP or other errors.
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject line</param>
        /// <param name="body">Email body (HTML supported)</param>
        /// <returns>A task representing the asynchronous operation</returns>

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"]
                    ?? throw new InvalidOperationException("SMTP Host not configured");

                int smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 587;

                var smtpUser = _configuration["Email:SmtpUser"]
                    ?? throw new InvalidOperationException("SMTP User not configured");

                var smtpPass = _configuration["Email:SmtpPass"]
                    ?? throw new InvalidOperationException("SMTP Password not configured");

                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@talentree.com";
                var fromName = _configuration["Email:FromName"] ?? "Talentree Platform";

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.High
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending email to {Email}", to);
                throw new InvalidOperationException($"SMTP Error: Failed to send email to {to}. Error: {smtpEx.Message}", smtpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                throw new InvalidOperationException($"Failed to send email to {to}. Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Email verification template with OTP code
        /// </summary>
        private string GetEmailVerificationTemplate(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: 50px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .email-header {{
            text-align: center;
            color: #4CAF50;
            margin-bottom: 20px;
        }}
        .email-header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .otp-box {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 10px;
            padding: 25px;
            text-align: center;
            margin: 30px 0;
        }}
        .otp-code {{
            background-color: #ffffff;
            border: 2px dashed #4CAF50;
            padding: 20px;
            text-align: center;
            font-size: 36px;
            font-weight: bold;
            letter-spacing: 8px;
            color: #333;
            margin: 10px 0;
            border-radius: 8px;
            font-family: 'Courier New', monospace;
        }}
        .email-body {{
            color: #333;
            line-height: 1.6;
            font-size: 15px;
        }}
        .warning-box {{
            background-color: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .warning-box p {{
            margin: 5px 0;
            color: #856404;
        }}
        .footer {{
            text-align: center;
            color: #777;
            font-size: 12px;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
        }}
        .emoji {{
            font-size: 24px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1><span class='emoji'>🌳</span> Welcome to Talentree!</h1>
        </div>
        
        <div class='email-body'>
            <p>Hi there,</p>
            
            <p>Thank you for registering on <strong>Talentree Platform</strong>. We're excited to have you on board!</p>
            
            <p>To complete your registration and verify your email address, please use the verification code below:</p>
        </div>
        
        <div class='otp-box'>
            <div class='otp-code'>{otpCode}</div>
        </div>
        
        <div class='email-body'>
            <p>⏱️ This code will expire in <strong>5 minutes</strong>.</p>
            
            <div class='warning-box'>
                <p>⚠️ <strong>Security Notice:</strong></p>
                <p>• Never share this code with anyone</p>
                <p>• Talentree team will never ask for your OTP code</p>
                <p>• If you didn't request this code, please ignore this email</p>
            </div>
            
            <p>Once verified, you'll have full access to our marketplace platform.</p>
            
            <p>Happy shopping! 🚀</p>
        </div>
        
        <div class='footer'>
            <p><strong>Talentree Platform</strong> - Your B2B/B2C Marketplace</p>
            <p>© 2026 Talentree. All rights reserved.</p>
            <p style='margin-top: 10px;'>This is an automated message, please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Password reset template with OTP code
        /// </summary>
        private string GetPasswordResetTemplate(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: 50px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .email-header {{
            text-align: center;
            color: #f44336;
            margin-bottom: 20px;
        }}
        .email-header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .otp-box {{
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            border-radius: 10px;
            padding: 25px;
            text-align: center;
            margin: 30px 0;
        }}
        .otp-code {{
            background-color: #ffffff;
            border: 2px dashed #f44336;
            padding: 20px;
            text-align: center;
            font-size: 36px;
            font-weight: bold;
            letter-spacing: 8px;
            color: #333;
            margin: 10px 0;
            border-radius: 8px;
            font-family: 'Courier New', monospace;
        }}
        .email-body {{
            color: #333;
            line-height: 1.6;
            font-size: 15px;
        }}
        .warning-box {{
            background-color: #f8d7da;
            border-left: 4px solid #f44336;
            padding: 15px;
            margin: 20px 0;
            border-radius: 5px;
        }}
        .warning-box p {{
            margin: 5px 0;
            color: #721c24;
        }}
        .footer {{
            text-align: center;
            color: #777;
            font-size: 12px;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #eee;
        }}
        .emoji {{
            font-size: 24px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1><span class='emoji'>🔒</span> Password Reset Request</h1>
        </div>
        
        <div class='email-body'>
            <p>Hi there,</p>
            
            <p>We received a request to reset your password on <strong>Talentree Platform</strong>.</p>
            
            <p>To proceed with resetting your password, please use the verification code below:</p>
        </div>
        
        <div class='otp-box'>
            <div class='otp-code'>{otpCode}</div>
        </div>
        
        <div class='email-body'>
            <p>⏱️ This code will expire in <strong>5 minutes</strong>.</p>
            
            <div class='warning-box'>
                <p>⚠️ <strong>Security Alert:</strong></p>
                <p>• If you didn't request a password reset, please ignore this email</p>
                <p>• Your password will remain unchanged if you don't use this code</p>
                <p>• Contact our support team immediately if you suspect suspicious activity</p>
            </div>
            
            <p>After entering the code, you'll be able to create a new password for your account.</p>
        </div>
        
        <div class='footer'>
            <p><strong>Talentree Platform</strong> - Secure Marketplace</p>
            <p>© 2026 Talentree. All rights reserved.</p>
            <p style='margin-top: 10px;'>This is an automated message, please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}