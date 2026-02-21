using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.DTOs.Admin.RawMaterial;
using Talentree.Core.Service.Contract;

namespace Talentree.API.Controllers.Admin
{
    /// <summary>
    /// Admin-only endpoints for managing raw materials.
    /// Admins can create, update, delete, restock, and upload images for materials.
    /// Business Owners browse these materials via the RawMaterialController.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminRawMaterialController : BaseApiController
    {
        private readonly IAdminRawMaterialService _materialService;

        public AdminRawMaterialController(IAdminRawMaterialService materialService)
        {
            _materialService = materialService;
        }

        /// <summary>
        /// Gets a paginated list of all raw materials including unavailable ones.
        /// Supports filtering by category, search term, and availability status.
        /// </summary>
        /// <param name="category">Optional category filter</param>
        /// <param name="search">Optional search term matched against name and description</param>
        /// <param name="isAvailable">Optional availability filter</param>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMaterials(
            [FromQuery] string? category,
            [FromQuery] string? search,
            [FromQuery] bool? isAvailable,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageIndex < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid pagination parameters."));

            var result = await _materialService.GetMaterialsAsync(category, search, isAvailable, pageIndex, pageSize);

            return Ok(ApiResponse<object>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} materials."
            ));
        }

        /// <summary>
        /// Gets full details of a single raw material.
        /// </summary>
        /// <param name="id">Material ID</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminRawMaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMaterialById(int id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);

            return Ok(ApiResponse<AdminRawMaterialDto>.SuccessResponse(
                data: material,
                message: "Material retrieved successfully."
            ));
        }

        /// <summary>
        /// Creates a new raw material linked to an active supplier.
        /// Availability is automatically set based on initial stock quantity.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AdminRawMaterialDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateMaterial([FromBody] CreateRawMaterialDto dto)
        {
            var material = await _materialService.CreateMaterialAsync(dto);

            return CreatedAtAction(
                nameof(GetMaterialById),
                new { id = material.Id },
                ApiResponse<AdminRawMaterialDto>.SuccessResponse(
                    data: material,
                    message: $"Material '{material.Name}' created successfully."
                ));
        }

        /// <summary>
        /// Partially updates a raw material. Only provided fields are updated.
        /// Setting stock to 0 automatically marks the material as unavailable.
        /// </summary>
        /// <param name="id">Material ID to update</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminRawMaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] UpdateRawMaterialDto dto)
        {
            var material = await _materialService.UpdateMaterialAsync(id, dto);

            return Ok(ApiResponse<AdminRawMaterialDto>.SuccessResponse(
                data: material,
                message: "Material updated successfully."
            ));
        }

        /// <summary>
        /// Soft deletes a raw material and marks it as unavailable.
        /// </summary>
        /// <param name="id">Material ID to delete</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            await _materialService.DeleteMaterialAsync(id);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Material deleted successfully."
            ));
        }

        /// <summary>
        /// Adds stock quantity to an existing material.
        /// Automatically re-enables the material if it was marked unavailable due to zero stock.
        /// </summary>
        /// <param name="id">Material ID to restock</param>
        [HttpPatch("{id:int}/restock")]
        [ProducesResponseType(typeof(ApiResponse<AdminRawMaterialDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestockMaterial(int id, [FromBody] RestockMaterialDto dto)
        {
            var material = await _materialService.RestockMaterialAsync(id, dto);

            return Ok(ApiResponse<AdminRawMaterialDto>.SuccessResponse(
                data: material,
                message: $"Stock updated. New quantity: {material.StockQuantity} {material.Unit}"
            ));
        }

        /// <summary>
        /// Uploads or replaces the image for a raw material.
        /// Accepted formats: JPG, PNG, WebP. Maximum size: 5MB.
        /// Old image is deleted automatically when replaced.
        /// </summary>
        /// <param name="id">Material ID to upload image for</param>
        [HttpPost("{id:int}/upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadImage(int id, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("No image file provided."));

            var imageUrl = await _materialService.UploadMaterialImageAsync(id, image);

            return Ok(ApiResponse<object>.SuccessResponse(
                data: new { imageUrl },
                message: "Image uploaded successfully."
            ));
        }
    }
}