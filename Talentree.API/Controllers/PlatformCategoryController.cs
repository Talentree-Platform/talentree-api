using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// FR-AD-31: Admin management of product categories and subcategories.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/admin/platform/categories")]
    [ApiController]
    public class PlatformCategoryController : BaseApiController
    {
        private readonly ICategoryManagementService _categoryService;

        public PlatformCategoryController(ICategoryManagementService categoryService)
        {
            _categoryService = categoryService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/categories
        // Returns all categories as a hierarchical tree (root → subcategories)
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PlatformCategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PlatformCategoryDto>>>> GetAllCategories()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            return Ok(ApiResponse<List<PlatformCategoryDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Count} categories"));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/categories/{id}
        // ═══════════════════════════════════════════════════════════
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PlatformCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PlatformCategoryDto>>> GetCategory(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(ApiResponse<PlatformCategoryDto>.SuccessResponse(
                data: result,
                message: "Category retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/platform/categories/{id}
        // Edit category name, description, icon, display order
        // ═══════════════════════════════════════════════════════════
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PlatformCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PlatformCategoryDto>>> UpdateCategory(
            int id, [FromBody] UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateCategoryAsync(id, dto, GetCurrentUserId());
            return Ok(ApiResponse<PlatformCategoryDto>.SuccessResponse(
                data: result,
                message: "Category updated successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/admin/platform/categories/subcategories
        // Create a subcategory under an existing parent category
        // ═══════════════════════════════════════════════════════════
        [HttpPost("subcategories")]
        [ProducesResponseType(typeof(ApiResponse<PlatformCategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PlatformCategoryDto>>> CreateSubCategory(
            [FromBody] CreateSubCategoryDto dto)
        {
            var result = await _categoryService.CreateSubCategoryAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetCategory), new { id = result.Id },
                ApiResponse<PlatformCategoryDto>.SuccessResponse(
                    data: result,
                    message: "Subcategory created successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/admin/platform/categories/reorder
        // Batch update display order for multiple categories
        // ═══════════════════════════════════════════════════════════
        [HttpPut("reorder")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ReorderCategories(
            [FromBody] ReorderCategoriesDto dto)
        {
            await _categoryService.ReorderCategoriesAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Categories reordered successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // PATCH: api/admin/platform/categories/{id}/toggle-disabled
        // Toggle the IsDisabled flag — hides category from users but preserves data
        // ═══════════════════════════════════════════════════════════
        [HttpPatch("{id:int}/toggle-disabled")]
        [ProducesResponseType(typeof(ApiResponse<PlatformCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PlatformCategoryDto>>> ToggleCategoryDisabled(int id)
        {
            var result = await _categoryService.ToggleCategoryDisabledAsync(id, GetCurrentUserId());
            var status = result.IsDisabled ? "disabled" : "enabled";
            return Ok(ApiResponse<PlatformCategoryDto>.SuccessResponse(
                data: result,
                message: $"Category {status} successfully"));
        }
    }
}
