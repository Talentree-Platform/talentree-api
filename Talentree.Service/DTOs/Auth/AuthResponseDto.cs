// Talentree.Service/DTOs/Auth/AuthResponseDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// Authentication response containing tokens and user info
    /// Returned after successful login, registration verification, or token refresh
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// JWT access token (short-lived, typically 15-60 minutes)
        /// Include in Authorization header: "Bearer {AccessToken}"
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh token (long-lived, typically 7-30 days)
        /// Store securely, use to get new access tokens
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// When access token expires (UTC)
        /// Client should refresh before this time
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Authenticated user information
        /// </summary>
        public UserInfoDto User { get; set; } = null!;
    }
}