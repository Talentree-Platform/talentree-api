// Talentree.Service/Contracts/IEmailService.cs

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Email sending service
    /// Handles sending OTPs, verification emails, notifications, etc.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends OTP code for email verification or password reset
        /// </summary>
        Task SendOtpAsync(string email, string otpCode, string subject);

        /// <summary>
        /// Sends generic email
        /// </summary>
        Task SendEmailAsync(string to, string subject, string body);
    }
}
