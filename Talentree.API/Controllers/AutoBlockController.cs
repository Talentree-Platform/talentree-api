using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.Entities;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.UserManagement;

namespace Talentree.API.Controllers
{
    [Route("api/admin/auto-blocks")]
    [Authorize(Roles = "Admin")]
    public class AutoBlockController : BaseApiController
    {
        private readonly IUserManagementService _userManagementService;

        public AutoBlockController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        /// <summary>
        /// Get pending auto-blocks for review
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<List<AutoBlockLog>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AutoBlockLog>>>> GetPendingAutoBlocks()
        {
            var logs = await _userManagementService.GetPendingAutoBlocksAsync();

            return Ok(ApiResponse<List<AutoBlockLog>>.SuccessResponse(
                data: logs,
                message: $"Retrieved {logs.Count} pending auto-block(s)"
            ));
        }

        /// <summary>
        /// Review auto-block
        /// </summary>
        [HttpPost("review")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ReviewAutoBlock([FromBody] AutoBlockReviewDto dto)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.ReviewAutoBlockAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: $"Auto-block reviewed. Decision: {dto.Decision}"
            ));
        }
    }
}