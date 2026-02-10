// Talentree.Service/DTOs/Auth/RefreshTokenDto.cs

namespace Talentree.Service.DTOs.Auth
{
    /// <summary>
    /// DTO for refreshing access token
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// Refresh token received from login or previous refresh
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}