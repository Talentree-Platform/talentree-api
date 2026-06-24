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
        public UserInfoDto? User { get; set; }

        /// <summary>
        /// Indicates if 2FA verification is required to complete authentication
        /// </summary>
        public bool RequiresTwoFactor { get; set; } = false;

        /// <summary>
        /// The multi-factor authentication provider type (e.g. Email)
        /// </summary>
        public string? TwoFactorProvider { get; set; }

        /// <summary>
        /// The user ID requesting authentication (useful for 2FA validation)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Indicates if a password change is forced on first login
        /// </summary>
        public bool RequiresPasswordChange { get; set; } = false;
    }
}