using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.UserManagement;

namespace Talentree.API.Controllers
{
    [Authorize]
    public class ComplaintController : BaseApiController
    {
        private readonly IUserManagementService _userManagementService;

        public ComplaintController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        /// <summary>
        /// Create a complaint
        /// Route: POST /api/complaint
        /// </summary>
        [HttpPost]  // ✅ No route specified - uses base route
        [ProducesResponseType(typeof(ApiResponse<ComplaintDto>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<ComplaintDto>>> CreateComplaint(
            [FromBody] CreateComplaintDto dto)
        {
            var reportedByUserId = GetCurrentUserId();
            var complaint = await _userManagementService.CreateComplaintAsync(dto, reportedByUserId);

            return Created($"/api/complaint/{complaint.Id}",
                ApiResponse<ComplaintDto>.SuccessResponse(
                    data: complaint,
                    message: "Complaint submitted successfully."
                ));
        }

        /// <summary>
        /// Get complaint by ID
        /// Route: GET /api/complaint/{id}
        /// </summary>
        [HttpGet("{id}")]  // ✅ Relative to base route
        [ProducesResponseType(typeof(ApiResponse<ComplaintDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ComplaintDto>>> GetComplaintById(int id)
        {
            var complaint = await _userManagementService.GetComplaintByIdAsync(id);

            return Ok(ApiResponse<ComplaintDto>.SuccessResponse(
                data: complaint,
                message: "Complaint retrieved successfully"
            ));
        }
    }
}