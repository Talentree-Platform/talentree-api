using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Auth;

namespace Talentree.API.Controllers
{
    public class AuthController : BaseApiController
    {


        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var message = await _authService.RegisterAsync(registerDto);
            return Ok(ApiResponse<string>.SuccessResponse(message));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var authResponse = await _authService.LoginAsync(loginDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var authResponse = await _authService.RefreshTokenAsync(refreshTokenDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            await _authService.LogoutAsync(refreshTokenDto.RefreshToken);
            return Ok(ApiResponse<string>.SuccessResponse("Logged out successfully"));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var authResponse = await _authService.VerifyEmailAsync(verifyEmailDto);

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Email verified successfully. You are now logged in."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return Ok(ApiResponse<string>.SuccessResponse("If the email exists, a password reset code has been sent"));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            await _authService.ResetPasswordAsync(resetPasswordDto);
            return Ok(ApiResponse<string>.SuccessResponse("Password reset successfully"));
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] ExternalLoginDto externalLoginDto)
        {
            var authResponse = await _authService.GoogleLoginAsync(externalLoginDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse));
        }

        [HttpPost("facebook-login")]
        public async Task<IActionResult> FacebookLogin([FromBody] ExternalLoginDto externalLoginDto)
        {
            var authResponse = await _authService.FacebookLoginAsync(externalLoginDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse));
        }


    }
}
