using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// FR-AD-32: Admin management of platform commission and fee settings.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/admin/platform/commission")]
    [ApiController]
    public class PlatformCommissionController : BaseApiController
    {
        private readonly ICommissionSettingService _commissionService;

        public PlatformCommissionController(ICommissionSettingService commissionService)
        {
            _commissionService = commissionService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/commission
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CommissionSettingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CommissionSettingDto>>> GetCommissionSettings()
        {
            var result = await _commissionService.GetCommissionSettingsAsync();
            return Ok(ApiResponse<CommissionSettingDto>.SuccessResponse(
                data: result,
                message: "Commission settings retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/platform/commission
        // Changes apply to NEW transactions only — not retroactive
        // ═══════════════════════════════════════════════════════════
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<CommissionSettingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CommissionSettingDto>>> UpdateCommissionSettings(
            [FromBody] UpdateCommissionSettingDto dto)
        {
            var result = await _commissionService.UpdateCommissionSettingsAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<CommissionSettingDto>.SuccessResponse(
                data: result,
                message: "Commission settings updated successfully. Changes apply to new transactions only."));
        }
    }
}
