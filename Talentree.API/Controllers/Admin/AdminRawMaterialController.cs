using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.DTOs.Admin.RawMaterial;
using Talentree.Core.Service.Contract;

namespace Talentree.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminRawMaterialController : BaseApiController
    {
        private readonly IAdminRawMaterialService _materialService;

        public AdminRawMaterialController(IAdminRawMaterialService materialService)
        {
            _materialService = materialService;
        }

        // GET: api/admin/raw-materials?category=Textiles&search=cotton&isAvailable=true&pageIndex=1&pageSize=20
        [HttpGet]
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

        // GET: api/admin/raw-materials/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMaterialById(int id)
        {
            var material = await _materialService.GetMaterialByIdAsync(id);

            return Ok(ApiResponse<AdminRawMaterialDto>.SuccessResponse(
                data: material,
                message: "Material retrieved successfully."
            ));
        }

        // POST: api/admin/raw-materials
        [HttpPost]
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

        // PUT: api/admin/raw-materials/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] UpdateRawMaterialDto dto)
        {
            var material = await _materialService.UpdateMaterialAsync(id, dto);

            return Ok(ApiResponse<AdminRawMaterialDto>.SuccessResponse(
                data: material,
                message: "Material updated successfully."
            ));
        }

        // DELETE: api/admin/raw-materials/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            await _materialService.DeleteMaterialAsync(id);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Material deleted successfully."
            ));
        }

        // PATCH: api/admin/raw-materials/{id}/restock
        [HttpPatch("{id:int}/restock")]
        public async Task<IActionResult> RestockMaterial(int id, [FromBody] RestockMaterialDto dto)
        {
            var material = await _materialService.RestockMaterialAsync(id, dto);

            return Ok(ApiResponse<AdminRawMaterialDto>.SuccessResponse(
                data: material,
                message: $"Stock updated. New quantity: {material.StockQuantity} {material.Unit}"
            ));
        }

        // POST: api/admin/raw-materials/{id}/upload-image
        [HttpPost("{id:int}/upload-image")]
        [Consumes("multipart/form-data")]
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
