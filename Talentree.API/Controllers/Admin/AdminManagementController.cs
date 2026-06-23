using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin;
using Talentree.Service.DTOs.Common;

namespace Talentree.API.Controllers.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/admin-management")]
    public class AdminManagementController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminManagementController(IAdminService adminService)
        {
            _adminService = adminService;
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
    }
}
