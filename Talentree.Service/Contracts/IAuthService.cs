// Talentree.Service/Contracts/IAuthService.cs
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Authentication service contract
    /// Contains all authentication-related use cases
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user
        /// Creates user account, assigns role, sends verification email
        /// </summary>
        Task<string> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Authenticates user and generates JWT tokens
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// Generates new access token using refresh token
        /// Implements token rotation (revokes old, creates new)
        /// </summary>
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

        /// <summary>
        /// Revokes refresh token (logout)
        /// Marks token as revoked in database
        /// </summary>
        Task LogoutAsync(string refreshToken);

        /// <summary>
        /// Verifies user email using OTP code
        /// Returns auth tokens upon successful verification
        /// </summary>
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);

        /// <summary>
        /// Sends OTP code for password reset
        /// </summary>
        Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);

        /// <summary>
        /// Resets user password using OTP code
        /// </summary>
        Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        /// <summary>
        /// Authenticates user via Google
        /// Creates account if doesn't exist
        /// </summary>
        Task<AuthResponseDto> GoogleLoginAsync(ExternalLoginDto externalLoginDto);

        /// <summary>
        /// Authenticates user via Facebook
        /// Creates account if doesn't exist
        /// </summary>
        Task<AuthResponseDto> FacebookLoginAsync(ExternalLoginDto externalLoginDto);
    }
}
