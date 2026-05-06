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
        /// Create a complaint against a user
        /// </summary>
        [HttpPost("complaints")]
        [ProducesResponseType(typeof(ApiResponse<ComplaintDto>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<ComplaintDto>>> CreateComplaint([FromBody] CreateComplaintDto dto)
        {
            var reportedByUserId = GetCurrentUserId();
            var complaint = await _userManagementService.CreateComplaintAsync(dto, reportedByUserId);

            return Created($"/api/complaints/{complaint.Id}",
                ApiResponse<ComplaintDto>.SuccessResponse(
                    data: complaint,
                    message: "Complaint submitted successfully. Our team will review it."
                ));
        }

        /// <summary>
        /// Get complaint by ID
        /// </summary>
        [HttpGet("complaints/{id}")]
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