using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talentree.API.Models;
using Talentree.Core;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Specifications;
using Guidy.Core.Specifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Auth;

namespace Talentree.API.Controllers
{
    [Authorize]
    [Route("api/account")]
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IAuditLogService _auditLogService;

        public AccountController(
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IAuditLogService auditLogService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _auditLogService = auditLogService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/account/security-status
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Retrieve the active user's security settings and session counts
        /// </summary>
        [HttpGet("security-status")]
        [ProducesResponseType(typeof(ApiResponse<SecurityStatusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<SecurityStatusDto>>> GetSecurityStatus()
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            // Count active session tokens
            var activeTokensSpec = new ActiveRefreshTokensForUserSpecification(userId);
            var activeSessionCount = await _unitOfWork.Repository<RefreshToken>().GetCountWithSpecificationsAsync(activeTokensSpec);

            // Last login info
            var lastSuccessfulSpec = new Talentree.Core.Specifications.AccountSettingsSpecifications.LastSuccessfulLoginSpecification(userId);
            var lastLogins = await _unitOfWork.Repository<LoginHistory>().GetAllWithSpecificationsAsync(lastSuccessfulSpec);
            var lastLoginRecord = lastLogins.FirstOrDefault();

            var statusDto = new SecurityStatusDto
            {
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                MustChangePassword = user.MustChangePassword,
                FailedAttempts = user.AccessFailedCount,
                LockoutEnd = user.LockoutEnd,
                LastLoginDate = lastLoginRecord?.LoginAt,
                LastLoginIp = lastLoginRecord?.IpAddress,
                ActiveSessionCount = activeSessionCount
            };

            return Ok(ApiResponse<SecurityStatusDto>.SuccessResponse(statusDto, "Security status retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/account/me/permissions
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Retrieve the current user's role and list of effective permissions
        /// </summary>
        [HttpGet("me/permissions")]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<UserPermissionsDto>>> GetMePermissions()
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Customer";

            // Map static permissions list based on role
            var permissions = new List<string>();
            if (role == "SuperAdmin")
            {
                permissions.AddRange(new[] { "FullAccess", "ManageAdmins", "ManageSecuritySettings", "ViewAuditLogs", "ViewLoginHistory" });
            }
            else if (role == "Admin")
            {
                permissions.AddRange(new[] { "ManagePlatform", "ManageProducts", "ManageOrders", "ViewTickets", "ResolveComplaints" });
            }
            else if (role == "SupportStaff")
            {
                permissions.AddRange(new[] { "ViewTickets", "ResolveComplaints" });
            }
            else if (role == "ContentManager")
            {
                permissions.AddRange(new[] { "ManageContent", "ManageProducts" });
            }

            var permissionsDto = new UserPermissionsDto
            {
                Role = role,
                Permissions = permissions
            };

            return Ok(ApiResponse<UserPermissionsDto>.SuccessResponse(permissionsDto, "Current permissions retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/account/request-enable-2fa
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Request enabling two-factor authentication (sends OTP email)
        /// </summary>
        [HttpPost("request-enable-2fa")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> RequestEnable2Fa()
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            if (user.IsTwoFactorEnabled)
                return BadRequest(ApiResponse<object>.ErrorResponse("2FA is already enabled."));

            // Generate OTP code
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Save in database
            var otpEntity = new OtpCode
            {
                UserId = user.Id,
                Code = code,
                Purpose = OtpPurpose.TwoFactorAuth,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };
            _unitOfWork.Repository<OtpCode>().Add(otpEntity);
            await _unitOfWork.CompleteAsync();

            // Send Email
            try
            {
                await _emailService.SendOtpAsync(user.Email!, code, OtpPurpose.TwoFactorAuth);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Failed to send verification email: {ex.Message}"));
            }

            // Audit
            await _auditLogService.LogActionAsync(
                userId: user.Id,
                adminId: user.Id,
                action: "Request Enable 2FA",
                reason: "User requested to enable two-factor authentication. Verification code sent.",
                entityType: "AppUser",
                entityId: user.Id
            );

            return Ok(ApiResponse<object>.SuccessResponse(message: "Verification code sent to your email."));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/account/confirm-enable-2fa
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Confirm verification OTP code to activate user two-factor setting
        /// </summary>
        [HttpPost("confirm-enable-2fa")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> ConfirmEnable2Fa([FromBody] ConfirmEnableTwoFactorDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            if (user.IsTwoFactorEnabled)
                return BadRequest(ApiResponse<object>.ErrorResponse("2FA is already enabled."));

            // Verify active OTP
            var spec = new OtpCodeSpecification(user.Id, dto.OtpCode, OtpPurpose.TwoFactorAuth);
            var otpEntity = await _unitOfWork.Repository<OtpCode>().GetByIdWithSpecificationsAsync(spec);

            if (otpEntity == null || !otpEntity.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid or expired verification code."));
            }

            // Mark as used
            otpEntity.IsUsed = true;
            otpEntity.UsedAt = DateTime.UtcNow;
            _unitOfWork.Repository<OtpCode>().Update(otpEntity);

            // Set enable 2FA
            var oldVal = user.IsTwoFactorEnabled;
            user.IsTwoFactorEnabled = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update user security settings."));
            }

            await _unitOfWork.CompleteAsync();

            // Audit
            await _auditLogService.LogActionAsync(
                userId: user.Id,
                adminId: user.Id,
                action: "Confirm Enable 2FA",
                reason: "User successfully confirmed and enabled two-factor authentication.",
                entityType: "AppUser",
                entityId: user.Id,
                beforeValues: $"{{\"isTwoFactorEnabled\": {oldVal}}}",
                afterValues: $"{{\"isTwoFactorEnabled\": true}}"
            );

            return Ok(ApiResponse<object>.SuccessResponse(message: "Two-factor authentication enabled successfully."));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/account/disable-2fa
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Disable two-factor authentication (requires current password verification)
        /// </summary>
        [HttpPost("disable-2fa")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> Disable2Fa([FromBody] DisableTwoFactorDto dto)
        {
            var userId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

            if (!user.IsTwoFactorEnabled)
                return BadRequest(ApiResponse<object>.ErrorResponse("2FA is not enabled."));

            // Validate current password
            var isPassValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!isPassValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid current password. Password confirmation is required to disable 2FA."));
            }

            var oldVal = user.IsTwoFactorEnabled;
            user.IsTwoFactorEnabled = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Failed to update user security settings."));
            }

            // Audit
            await _auditLogService.LogActionAsync(
                userId: user.Id,
                adminId: user.Id,
                action: "Disable 2FA",
                reason: "User successfully disabled two-factor authentication.",
                entityType: "AppUser",
                entityId: user.Id,
                beforeValues: $"{{\"isTwoFactorEnabled\": {oldVal}}}",
                afterValues: $"{{\"isTwoFactorEnabled\": false}}"
            );

            return Ok(ApiResponse<object>.SuccessResponse(message: "Two-factor authentication disabled successfully."));
        }
    }
}
