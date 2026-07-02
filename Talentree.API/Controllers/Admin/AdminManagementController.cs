using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin;
using Talentree.Service.DTOs.Auth;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.UserManagement;

namespace Talentree.API.Controllers.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/admin-management")]
    public class AdminManagementController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly ISecuritySettingsService _securitySettingsService;
        private readonly IUserManagementService _userManagementService;
        private readonly IAuditLogService _auditLogService;

        public AdminManagementController(
            IAdminService adminService,
            ISecuritySettingsService securitySettingsService,
            IUserManagementService userManagementService,
            IAuditLogService auditLogService)
        {
            _adminService = adminService;
            _securitySettingsService = securitySettingsService;
            _userManagementService = userManagementService;
            _auditLogService = auditLogService;
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin-management/create
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Create a new admin user with a specific role (Super Admin only)
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse<AdminDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<AdminDto>>> CreateAdmin(
            [FromBody] CreateAdminDto dto)
        {
            var admin = await _adminService.CreateAdminAsync(dto, GetCurrentUserId());

            return CreatedAtAction(
                nameof(GetAllAdmins),
                new { },
                ApiResponse<AdminDto>.SuccessResponse(
                    data: admin,
                    message: $"Admin '{admin.FullName}' created successfully with role '{admin.Role}'"
                ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin-management/admins
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Get all admin users with their roles (Super Admin only)
        /// </summary>
        [HttpGet("admins")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<List<AdminDto>>>> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();

            return Ok(ApiResponse<List<AdminDto>>.SuccessResponse(
                data: admins,
                message: $"Retrieved {admins.Count} admin(s)"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin-management/admins/{adminId}
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Edit admin profile information (Super Admin only)
        /// </summary>
        [HttpPut("admins/{adminId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<AdminDto>>> EditAdmin(
            string adminId, 
            [FromBody] EditAdminDto dto)
        {
            var admin = await _adminService.EditAdminAsync(adminId, dto, GetCurrentUserId());

            return Ok(ApiResponse<AdminDto>.SuccessResponse(
                data: admin,
                message: "Admin details updated successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin-management/admins/{adminId}/role
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Change an admin's role (Super Admin only)
        /// </summary>
        [HttpPut("admins/{adminId}/role")]
        [ProducesResponseType(typeof(ApiResponse<AdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<AdminDto>>> ChangeAdminRole(
            string adminId,
            [FromBody] ChangeAdminRoleDto dto)
        {
            var admin = await _adminService.ChangeAdminRoleAsync(adminId, dto, GetCurrentUserId());

            return Ok(ApiResponse<AdminDto>.SuccessResponse(
                data: admin,
                message: $"Admin role changed to '{dto.Role}' successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin-management/admins/{adminId}/reset-password
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Reset an admin user's password (Super Admin only)
        /// </summary>
        [HttpPost("admins/{adminId}/reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> ResetAdminPassword(
            string adminId,
            [FromBody] ResetAdminPasswordDto dto)
        {
            await _adminService.ResetAdminPasswordAsync(adminId, dto, GetCurrentUserId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Admin password reset successfully. User's sessions have been revoked."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin-management/admins/{adminId}/deactivate
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Deactivate an admin user account (Super Admin only)
        /// </summary>
        [HttpPost("admins/{adminId}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateAdmin(string adminId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == adminId)
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Cannot deactivate your own account",
                    errors: new List<string> { "Use another Super Admin account to deactivate this account" }
                ));

            await _adminService.DeactivateAdminAsync(adminId, currentUserId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Admin account deactivated successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin-management/admins/{adminId}/reactivate
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Reactivate an admin user account (Super Admin only)
        /// </summary>
        [HttpPost("admins/{adminId}/reactivate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> ReactivateAdmin(string adminId)
        {
            await _adminService.ReactivateAdminAsync(adminId, GetCurrentUserId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Admin account reactivated successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin-management/security-settings
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Get global security settings (Super Admin only)
        /// </summary>
        [HttpGet("security-settings")]
        [ProducesResponseType(typeof(ApiResponse<SecuritySettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<SecuritySettingsDto>>> GetSecuritySettings()
        {
            var settings = await _securitySettingsService.GetSecuritySettingsAsync();
            return Ok(ApiResponse<SecuritySettingsDto>.SuccessResponse(
                data: settings,
                message: "Security settings retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin-management/security-settings
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Update global security settings (Super Admin only)
        /// </summary>
        [HttpPut("security-settings")]
        [ProducesResponseType(typeof(ApiResponse<SecuritySettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<SecuritySettingsDto>>> UpdateSecuritySettings(
            [FromBody] UpdateSecuritySettingsDto dto)
        {
            var settings = await _securitySettingsService.UpdateSecuritySettingsAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<SecuritySettingsDto>.SuccessResponse(
                data: settings,
                message: "Security settings updated successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin-management/audit-logs
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Retrieve filtered and paginated audit logs (Super Admin only)
        /// </summary>
        [HttpGet("audit-logs")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<UserActionLogDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<Pagination<UserActionLogDto>>>> GetAuditLogs(
            [FromQuery] UserActionLogFilterDto filter)
        {
            var logs = await _userManagementService.GetAuditLogsAsync(filter);
            return Ok(ApiResponse<Pagination<UserActionLogDto>>.SuccessResponse(
                data: logs,
                message: "Audit logs retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin-management/audit-logs/export
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Export filtered audit logs as a CSV file (Super Admin only)
        /// </summary>
        [HttpGet("audit-logs/export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportAuditLogs([FromQuery] UserActionLogFilterDto filter)
        {
            var csvBytes = await _userManagementService.ExportAuditLogsToCsvAsync(filter);
            return File(csvBytes, "text/csv", $"audit_logs_export_{System.DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin-management/roles
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Retrieve the read-only roles permissions matrix (Super Admin only)
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(ApiResponse<List<UserPermissionsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<List<UserPermissionsDto>>>> GetRolesPermissionsMatrix()
        {
            var matrix = new List<UserPermissionsDto>
            {
                new UserPermissionsDto
                {
                    Role = "SuperAdmin",
                    Permissions = new List<string> { "FullAccess", "ManageAdmins", "ManageSecuritySettings", "ViewAuditLogs", "ViewLoginHistory" }
                },
                new UserPermissionsDto
                {
                    Role = "Admin",
                    Permissions = new List<string> { "ManagePlatform", "ManageProducts", "ManageOrders", "ViewTickets", "ResolveComplaints" }
                },
                new UserPermissionsDto
                {
                    Role = "SupportStaff",
                    Permissions = new List<string> { "ViewTickets", "ResolveComplaints" }
                },
                new UserPermissionsDto
                {
                    Role = "ContentManager",
                    Permissions = new List<string> { "ManageContent", "ManageProducts" }
                }
            };

            await _auditLogService.LogActionAsync(
                userId: GetCurrentUserId(),
                adminId: GetCurrentUserId(),
                action: "View Roles Permissions Matrix",
                reason: "SuperAdmin queried the global role permissions matrix.",
                entityType: "Role",
                entityId: "All"
            );

            return Ok(ApiResponse<List<UserPermissionsDto>>.SuccessResponse(matrix, "Roles permissions matrix retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin-management/admins/{adminId}/revoke-sessions
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Revoke all active sessions for the specified admin account (Super Admin only)
        /// </summary>
        [HttpPost("admins/{adminId}/revoke-sessions")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> RevokeSessions(string adminId)
        {
            await _adminService.RevokeSessionsAsync(adminId, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Admin account sessions revoked successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin-management/admins/{adminId}/unlock
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Unlock a locked admin account and reset failed login count (Super Admin only)
        /// </summary>
        [HttpPost("admins/{adminId}/unlock")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> UnlockAdmin(string adminId)
        {
            await _adminService.UnlockAdminAsync(adminId, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Admin account unlocked successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin-management/login-history
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Retrieve filtered and paginated login history logs (Super Admin only)
        /// </summary>
        [HttpGet("login-history")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<AdminLoginHistoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<Pagination<AdminLoginHistoryDto>>>> GetLoginHistory(
            [FromQuery] LoginHistoryFilterDto filter)
        {
            var history = await _userManagementService.GetLoginHistoryAsync(filter);
            return Ok(ApiResponse<Pagination<AdminLoginHistoryDto>>.SuccessResponse(
                data: history,
                message: "Login history logs retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin-management/login-history/export
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Export filtered login history logs as a CSV file (Super Admin only)
        /// </summary>
        [HttpGet("login-history/export")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportLoginHistory([FromQuery] LoginHistoryFilterDto filter)
        {
            var csvBytes = await _userManagementService.ExportLoginHistoryToCsvAsync(filter);
            return File(csvBytes, "text/csv", $"login_history_export_{System.DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}
