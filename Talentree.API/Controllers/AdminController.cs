// Talentree.API/Controllers/AdminController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Admin;

namespace Talentree.API.Controllers
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
    }
}