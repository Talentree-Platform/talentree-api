using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Products;

namespace Talentree.API.Controllers
{
    [Authorize(Roles = "BusinessOwner")]
    public class BusinessOwnerProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public BusinessOwnerProductsController(IProductService productService)
        {
            _productService = productService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ═══════════════════════════════════════════════════════════
        // GET: api/business-owner-products
        // FR-BO-07: Product listing with search, filter, sort, pagination
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<ProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<ProductDto>>>> GetMyProducts(
            [FromQuery] ProductFilterDto filter)
        {
            var result = await _productService.GetMyProductsAsync(GetUserId(), filter);

            return Ok(ApiResponse<Pagination<ProductDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} products " +
                         $"(Page {result.PageIndex} of {result.TotalPages})"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/business-owner-products/{id}
        // ═══════════════════════════════════════════════════════════
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            var result = await _productService.GetProductByIdAsync(id, GetUserId());

            return Ok(ApiResponse<ProductDto>.SuccessResponse(
                data: result,
                message: "Product retrieved successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/business-owner-products
        // FR-BO-08: Create new product
        // ═══════════════════════════════════════════════════════════
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
            [FromForm] CreateProductDto dto,
            [FromForm] List<IFormFile> images)
        {
            var result = await _productService.CreateProductAsync(dto, images, GetUserId());

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = result.Id },
                ApiResponse<ProductDto>.SuccessResponse(
                    data: result,
                    message: "Product submitted successfully and is pending admin approval"
                )
            );
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/business-owner-products/{id}
        // FR-BO-09: Update product
        // ═══════════════════════════════════════════════════════════
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(
            int id,
            [FromForm] UpdateProductDto dto,
            [FromForm] List<IFormFile>? newImages)
        {
            var result = await _productService.UpdateProductAsync(id, dto, newImages, GetUserId());

            var message = result.Status == Core.Enums.ProductStatus.PendingApproval
                ? "Product updated and sent for re-approval"
                : "Product updated successfully";

            return Ok(ApiResponse<ProductDto>.SuccessResponse(
                data: result,
                message: message
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // DELETE: api/business-owner-products/{id}
        // FR-BO-10: Soft delete product
        // ═══════════════════════════════════════════════════════════
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id, GetUserId());

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Product deleted successfully"
            ));
        }
    }
}