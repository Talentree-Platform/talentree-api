// Talentree.API/Controllers/AdminController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin;
using Talentree.Service.DTOs.Common;
using Talentree.Service.Services;

namespace Talentree.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/business-owners/pending?pageIndex=1&pageSize=20
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Get paginated list of pending business owner applications
        /// </summary>
        /// <param name="pageIndex">Page number (1-based, default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
        [HttpGet("business-owners/pending")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<BusinessOwnerApplicationDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<BusinessOwnerApplicationDto>>>> GetPendingBusinessOwners(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            // ✅ Validate pagination parameters
            if (pageIndex < 1)
            {
                return BadRequest(ApiResponse<Pagination<BusinessOwnerApplicationDto>>.ErrorResponse(
                    "Page index must be greater than or equal to 1",
                    errors: new List<string> { "pageIndex: Must be >= 1" }
                ));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(ApiResponse<Pagination<BusinessOwnerApplicationDto>>.ErrorResponse(
                    "Page size must be between 1 and 100",
                    errors: new List<string> { "pageSize: Must be between 1 and 100" }
                ));
            }


            var result = await _adminService.GetPendingBusinessOwnersAsync(pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<BusinessOwnerApplicationDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} pending applications (Page {result.PageIndex} of {result.TotalPages})"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/business-owners/{id}
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Get business owner application details
        /// </summary>
        [HttpGet("business-owners/{profileId}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessOwnerApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessOwnerApplicationDto>>> GetBusinessOwnerDetails(int profileId)
        {
            var result = await _adminService.GetBusinessOwnerDetailsAsync(profileId);

            return Ok(ApiResponse<BusinessOwnerApplicationDto>.SuccessResponse(
                data: result,
                message: "Business owner details retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin/business-owners/approve
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Approve a business owner application
        /// </summary>
        [HttpPost("business-owners/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ApproveBusinessOwner(ApproveBusinessOwnerDto dto)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _adminService.ApproveBusinessOwnerAsync(dto, adminUserId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Business owner approved successfully. Approval email sent."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin/business-owners/reject
        // ═══════════════════════════════════════════════════════════
        /// <summary>
        /// Reject a business owner application
        /// </summary>
        [HttpPost("business-owners/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RejectBusinessOwner(RejectBusinessOwnerDto dto)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _adminService.RejectBusinessOwnerAsync(dto, adminUserId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Business owner application rejected. Rejection email sent."
            ));
        }


        // ═══════════════════════════════════════════════════════════
        // ADMIN MANAGEMENT ENDPOINTS
        // ═══════════════════════════════════════════════════════════

        [HttpPost("create-admin")]
        [Authorize(Roles = "Admin")] // ⭐ Only admins can create admins
        [ProducesResponseType(typeof(ApiResponse<AdminDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<AdminDto>>> CreateAdmin(
            [FromBody] CreateAdminDto dto)
        {
            var admin = await _adminService.CreateAdminAsync(dto);

            return CreatedAtAction(
                nameof(GetAllAdmins),
                new { },
                ApiResponse<AdminDto>.SuccessResponse(
                    data: admin,
                    message: $"Admin '{admin.FullName}' created successfully"
                ));
        }

        /// <summary>
        /// Get all admin users (Admin only)
        /// </summary>
        [HttpGet("admins")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<AdminDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminDto>>>> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();

            return Ok(ApiResponse<List<AdminDto>>.SuccessResponse(
                data: admins,
                message: $"Retrieved {admins.Count} admin(s)"
            ));
        }

        /// <summary>
        /// Deactivate admin account (Admin only)
        /// </summary>
        [HttpPost("admins/{adminId}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeactivateAdmin(string adminId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (currentUserId == adminId)
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Cannot deactivate your own account",
                    errors: new List<string> { "Use another admin account to deactivate this account" }
                ));

            await _adminService.DeactivateAdminAsync(adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Admin deactivated successfully"
            ));
        }

        /// <summary>
        /// Reactivate admin account (Admin only)
        /// </summary>
        [HttpPost("admins/{adminId}/reactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ReactivateAdmin(string adminId)
        {
            await _adminService.ReactivateAdminAsync(adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Admin reactivated successfully"
            ));
        }


    }
}