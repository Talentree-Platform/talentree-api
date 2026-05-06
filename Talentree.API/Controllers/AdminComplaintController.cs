using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.UserManagement;

namespace Talentree.API.Controllers
{
    [Route("api/admin/complaints")]
    [Authorize(Roles = "Admin")]
    public class AdminComplaintController : BaseApiController
    {
        private readonly IUserManagementService _userManagementService;

        public AdminComplaintController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        /// <summary>
        /// Get all complaints
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<ComplaintDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<ComplaintDto>>>> GetComplaints(
            [FromQuery] string? reportedUserId = null,
            [FromQuery] ComplaintStatus? status = null,
            [FromQuery] ViolationType? violationType = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _userManagementService.GetComplaintsAsync(
                reportedUserId, status, violationType, pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<ComplaintDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} complaint(s)"
            ));
        }

        /// <summary>
        /// Resolve complaint
        /// </summary>
        [HttpPost("resolve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ResolveComplaint([FromBody] ResolveComplaintDto dto)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.ResolveComplaintAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Complaint resolved successfully. Reporter has been notified."
            ));
        }

        /// <summary>
        /// Reject complaint
        /// </summary>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> RejectComplaint(
            int id,
            [FromBody] string reason)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.RejectComplaintAsync(id, reason, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Complaint rejected. Reporter has been notified."
            ));
        }
    }
}