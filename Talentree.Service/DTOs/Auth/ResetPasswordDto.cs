// Talentree.Service/DTOs/Auth/ResetPasswordDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// DTO for resetting password with OTP
    /// </summary>
    public class ResetPasswordDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 6-digit OTP code sent to email
        /// </summary>
        public string OtpCode { get; set; } = string.Empty;

        /// <summary>
        /// New password (will be hashed)
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// Confirm new password (must match NewPassword)
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}