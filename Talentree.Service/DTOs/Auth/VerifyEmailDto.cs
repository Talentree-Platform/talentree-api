// Talentree.Service/DTOs/Auth/VerifyEmailDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// DTO for email verification
    /// </summary>
    public class VerifyEmailDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 6-digit OTP code sent to email
        /// </summary>
        public string OtpCode { get; set; } = string.Empty;
    }
}