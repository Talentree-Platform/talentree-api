// Talentree.Service/DTOs/Auth/ForgotPasswordDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// DTO for forgot password request
    /// </summary>
    public class ForgotPasswordDto
    {
        /// <summary>
        /// User's email address
        /// OTP will be sent to this email if account exists
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}