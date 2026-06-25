using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// FR-AD-34: Admin management of tax configuration.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/admin/platform/tax")]
    [ApiController]
    public class PlatformTaxController : BaseApiController
    {
        private readonly ITaxSettingsService _taxService;

        public PlatformTaxController(ITaxSettingsService taxService)
        {
            _taxService = taxService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/tax
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<TaxSettingsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<TaxSettingsDto>>> GetTaxSettings()
        {
            var result = await _taxService.GetTaxSettingsAsync();
            return Ok(ApiResponse<TaxSettingsDto>.SuccessResponse(
                data: result,
                message: "Tax settings retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/platform/tax
        // ═══════════════════════════════════════════════════════════
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<TaxSettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TaxSettingsDto>>> UpdateTaxSettings(
            [FromBody] UpdateTaxSettingsDto dto)
        {
            var result = await _taxService.UpdateTaxSettingsAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<TaxSettingsDto>.SuccessResponse(
                data: result,
                message: "Tax settings updated successfully"));
        }
    }
}
