
using Talentree.Core.Entities.Identity;

namespace Talentree.Service.Contracts
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends OTP code for email verification or password reset
        /// </summary>
        /// <param name="email">Recipient email address</param>
        /// <param name="otpCode">6-digit OTP code</param>
        /// <param name="purpose">Purpose: EmailVerification or PasswordReset</param>
        Task SendOtpAsync(string email, string otpCode, OtpPurpose purpose);

        /// <summary>
        /// Sends generic email
        /// </summary>
        Task SendEmailAsync(string to, string subject, string body);
    }
}