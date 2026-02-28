using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.DTOs.RawMaterial;
using Talentree.Service.Contracts;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Business Owner-facing endpoints for browsing the raw material store.
    /// Materials are automatically filtered by the BO's business category.
    /// </summary>
    [Authorize(Roles = "BusinessOwner,Admin")]
    public class RawMaterialController : BaseApiController
    {
        private readonly IRawMaterialService _rawMaterialService;

        public RawMaterialController(IRawMaterialService rawMaterialService)
        {
            _rawMaterialService = rawMaterialService;
        }

        /// <summary>
        /// Gets a paginated list of available raw materials.
        /// Results are filtered by the authenticated BO's business category automatically.
        /// Supports additional filtering by category and free-text search.
        /// </summary>
        /// <param name="category">Optional sub-category filter within the BO's business category</param>
        /// <param name="search">Optional search term matched against material name and description</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Gets full details of a single available raw material.
        /// Used for the material detail page before adding to basket.
        /// </summary>
        /// <param name="id">Material ID</param>
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
