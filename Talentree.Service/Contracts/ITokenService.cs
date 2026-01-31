using Talentree.Core.Entities.Identity;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user, List<string> roles);

    /// <summary>
    /// Generates a refresh token entity for storing in DB
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns true if valid
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// Hashes a refresh token string before storing
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Verifies refresh token hash
    /// </summary>
    bool VerifyRefreshToken(string token, string tokenHash);
    public int GetRefreshTokenExpiryDays();
    public int GetAccessTokenExpiryMinutes();
}
