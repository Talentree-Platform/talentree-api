using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// FR-AD-33: Admin management of shipping configuration.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/admin/platform/shipping")]
    [ApiController]
    public class PlatformShippingController : BaseApiController
    {
        private readonly IShippingSettingsService _shippingService;

        public PlatformShippingController(IShippingSettingsService shippingService)
        {
            _shippingService = shippingService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/shipping
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ShippingSettingsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ShippingSettingsDto>>> GetShippingSettings()
        {
            var result = await _shippingService.GetShippingSettingsAsync();
            return Ok(ApiResponse<ShippingSettingsDto>.SuccessResponse(
                data: result,
                message: "Shipping settings retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/platform/shipping
        // ═══════════════════════════════════════════════════════════
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<ShippingSettingsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ShippingSettingsDto>>> UpdateShippingSettings(
            [FromBody] UpdateShippingSettingsDto dto)
        {
            var result = await _shippingService.UpdateShippingSettingsAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<ShippingSettingsDto>.SuccessResponse(
                data: result,
                message: "Shipping settings updated successfully"));
        }
    }
}
