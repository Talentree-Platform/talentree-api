// Talentree.Service/Contracts/ITokenService.cs
using Talentree.Service.DTOs.Auth;
using Talentree.Core.Entities.Identity;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// JWT and refresh token generation & validation service
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates JWT access token for a user
        /// </summary>
        string GenerateAccessToken(AppUser user, List<string> roles);

        /// <summary>
        /// Generates a refresh token for a user
        /// </summary>
        RefreshToken GenerateRefreshToken(string userId);

        /// <summary>
        /// Validates a JWT token and returns claims principal
        /// </summary>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Hashes a refresh token before storing
        /// </summary>
        string HashToken(string token);

        /// <summary>
        /// Verifies refresh token hash
        /// </summary>
        bool VerifyRefreshToken(string token, string tokenHash);
    }
}
