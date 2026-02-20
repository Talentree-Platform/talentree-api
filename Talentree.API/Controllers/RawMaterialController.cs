using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Core.DTOs.RawMaterial;
using Talentree.Core.Service.Contract;

namespace Talentree.API.Controllers
{
    [Authorize(Roles = "BusinessOwner,Admin")]
    public class RawMaterialController : BaseApiController
    {
        private readonly IRawMaterialService _rawMaterialService;

        public RawMaterialController(IRawMaterialService rawMaterialService)
        {
            _rawMaterialService = rawMaterialService;
        }

        // GET: api/raw-materials?category=Textiles&search=cotton&pageIndex=1&pageSize=20
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMaterials(
            [FromQuery] string? category,
            [FromQuery] string? search,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageIndex < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid pagination parameters."));

            var result = await _rawMaterialService.GetMaterialsAsync(
                GetCurrentUserId(), category, search, pageIndex, pageSize);

            return Ok(ApiResponse<object>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} materials."
            ));
        }

        // GET: api/raw-materials/{id}
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<RawMaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMaterialById(int id)
        {
            var material = await _rawMaterialService.GetMaterialByIdAsync(id);

            return Ok(ApiResponse<RawMaterialDto>.SuccessResponse(
                data: material,
                message: "Material retrieved successfully."
            ));
        }

        private string GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}
