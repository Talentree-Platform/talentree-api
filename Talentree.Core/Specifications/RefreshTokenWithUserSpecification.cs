
using Talentree.Core.Entities.Identity;
using Talentree.Core.Specifications;


namespace Guidy.Core.Specifications;

/// <summary>
/// Specification: Get refresh token by token hash with user included
/// </summary>
public class RefreshTokenWithUserSpecification : BaseSpecifications<RefreshToken>
{
    public RefreshTokenWithUserSpecification(string tokenHash)
        : base(rt => rt.TokenHash == tokenHash)
    {
        AddInclude(rt => rt.User);
    }
}

/// <summary>
/// Specification: Get active refresh tokens for a user
/// </summary>
public class ActiveRefreshTokensForUserSpecification : BaseSpecifications<RefreshToken>
{
    public ActiveRefreshTokensForUserSpecification(string userId)
        : base(rt => rt.UserId == userId &&
                     rt.RevokedAt == null &&
                     rt.ExpiresAt > DateTime.UtcNow)
    {
        AddOrderByDescending(rt => rt.CreatedAt);
    }
}

/// <summary>
/// Specification: Get expired refresh tokens
/// </summary>
public class ExpiredRefreshTokensSpecification : BaseSpecifications<RefreshToken>
{
    public ExpiredRefreshTokensSpecification()
        : base(rt => rt.ExpiresAt < DateTime.UtcNow)
    {
    }
}