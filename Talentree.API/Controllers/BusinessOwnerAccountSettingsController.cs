using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.AccountSettings;

namespace Talentree.API.Controllers
{
    [Authorize(Roles = "BusinessOwner")]
    public class BusinessOwnerAccountSettingsController : BaseApiController
    {
        private readonly IAccountSettingsService _accountSettingsService;

        public BusinessOwnerAccountSettingsController(IAccountSettingsService accountSettingsService)
        {
            _accountSettingsService = accountSettingsService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // ═══════════════════════════════════════════
        // FR-BO-32: Profile Management
        // ═══════════════════════════════════════════

        // GET: api/business-owner-settings/profile
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<ProfileDto>>> GetProfile()
        {
            var result = await _accountSettingsService.GetProfileAsync(GetUserId());
            return Ok(ApiResponse<ProfileDto>.SuccessResponse(result, "Profile retrieved successfully"));
        }

        // PUT: api/business-owner-settings/profile
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfile(
            [FromForm] UpdateProfileDto dto,
            [FromForm] IFormFile? profilePhoto,
            [FromForm] IFormFile? businessLogo)
        {
            var result = await _accountSettingsService.UpdateProfileAsync(
                GetUserId(), dto, profilePhoto, businessLogo);
            return Ok(ApiResponse<ProfileDto>.SuccessResponse(result, "Profile updated successfully"));
        }

        // ═══════════════════════════════════════════
        // FR-BO-33: Payment Information
        // ═══════════════════════════════════════════

        // POST: api/business-owner-settings/payment-info/view
        // (POST because it requires password in body)
        [HttpPost("payment-info/view")]
        public async Task<ActionResult<ApiResponse<PaymentInfoDto>>> GetPaymentInfo(
            [FromBody] ViewPaymentInfoDto dto)
        {
            var result = await _accountSettingsService.GetPaymentInfoAsync(GetUserId(), dto.CurrentPassword);
            return Ok(ApiResponse<PaymentInfoDto>.SuccessResponse(result, "Payment info retrieved"));
        }

        // PUT: api/business-owner-settings/payment-info
        [HttpPut("payment-info")]
        public async Task<ActionResult<ApiResponse<object>>> UpsertPaymentInfo(
            [FromBody] UpsertPaymentInfoDto dto)
        {
            await _accountSettingsService.UpsertPaymentInfoAsync(GetUserId(), dto, GetIpAddress());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Payment information saved successfully"));
        }

        // ═══════════════════════════════════════════
        // FR-BO-34: Security Settings
        // ═══════════════════════════════════════════

        // PUT: api/business-owner-settings/security/change-password
        [HttpPut("security/change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            await _accountSettingsService.ChangePasswordAsync(GetUserId(), dto);
            return Ok(ApiResponse<object>.SuccessResponse(message: "Password changed successfully"));
        }

        // GET: api/business-owner-settings/security/login-history
        [HttpGet("security/login-history")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<LoginHistoryDto>>>> GetLoginHistory(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _accountSettingsService.GetLoginHistoryAsync(GetUserId(), pageIndex, pageSize);
            return Ok(ApiResponse<IReadOnlyList<LoginHistoryDto>>.SuccessResponse(result, "Login history retrieved"));
        }

        // POST: api/business-owner-settings/security/revoke-other-sessions
        [HttpPost("security/revoke-other-sessions")]
        public async Task<ActionResult<ApiResponse<object>>> RevokeOtherSessions(
            [FromBody] RevokeSessionsDto dto)
        {
            await _accountSettingsService.RevokeAllOtherSessionsAsync(GetUserId(), dto.CurrentRefreshToken);
            return Ok(ApiResponse<object>.SuccessResponse(message: "All other sessions have been logged out"));
        }

        // ═══════════════════════════════════════════
        // FR-BO-35: Preferences
        // ═══════════════════════════════════════════

        // GET: api/business-owner-settings/preferences
        [HttpGet("preferences")]
        public async Task<ActionResult<ApiResponse<PreferencesDto>>> GetPreferences()
        {
            var result = await _accountSettingsService.GetPreferencesAsync(GetUserId());
            return Ok(ApiResponse<PreferencesDto>.SuccessResponse(result, "Preferences retrieved"));
        }

        // PUT: api/business-owner-settings/preferences
        [HttpPut("preferences")]
        public async Task<ActionResult<ApiResponse<PreferencesDto>>> UpdatePreferences(
            [FromBody] UpdatePreferencesDto dto)
        {
            var result = await _accountSettingsService.UpdatePreferencesAsync(GetUserId(), dto);
            return Ok(ApiResponse<PreferencesDto>.SuccessResponse(result, "Preferences updated successfully"));
        }
    }
}