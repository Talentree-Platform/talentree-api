// Talentree.API/Controllers/AdminController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Core.Specifications.ProductSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin;
using Talentree.Service.DTOs.Admin.Product;
using Talentree.Service.DTOs.Common;
using Talentree.Service.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Talentree.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminProductController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        private string GetCurrentUserId() =>
              User.FindFirstValue(ClaimTypes.NameIdentifier)!;



        [HttpGet("products/pending")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<PendingProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<PendingProductDto>>>> GetPendingProducts(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _adminService.GetPendingProductsAsync(pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<PendingProductDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} pending product(s)"
            ));
        }


        [HttpPost("products/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ApproveProduct(
            [FromBody] ApproveProductDto dto)
        {
            var adminId = GetCurrentUserId();
            await _adminService.ApproveProductAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Product approved successfully. Notification sent to business owner."
            ));
        }

        [HttpPost("products/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RejectProduct(
            [FromBody] RejectProductDto dto)
        {
            var adminId = GetCurrentUserId();
            await _adminService.RejectProductAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Product rejected. Notification sent to business owner."
            ));
        }
    }
}