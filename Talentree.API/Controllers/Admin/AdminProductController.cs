// Talentree.API/Controllers/Admin/AdminProductController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin.Product;
using Talentree.Service.DTOs.Common;

namespace Talentree.API.Controllers.Admin
{
    /// <summary>
    /// Admin endpoints for product management.
    /// FR-AD-09: Product Approval Queue
    /// FR-AD-10: Product Moderation
    /// FR-AD-11: Low Stock Alerts
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminProductController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly IAdminProductService _adminProductService;

        public AdminProductController(IAdminService adminService, IAdminProductService adminProductService)
        {
            _adminService = adminService;
            _adminProductService = adminProductService;
        }

        private string GetCurrentUserId() =>
              User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ═══════════════════════════════════════════════════════════
        // FR-AD-09: Product Approval Queue
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get paginated list of products pending approval.</summary>
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

        /// <summary>Get full product detail for the review panel (any status, including deleted).</summary>
        [HttpGet("products/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminProductDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminProductDetailDto>>> GetProductDetail(int id)
        {
            var result = await _adminProductService.GetProductDetailAsync(id);
            return Ok(ApiResponse<AdminProductDetailDto>.SuccessResponse(data: result));
        }

        /// <summary>Approve a single pending product.</summary>
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

        /// <summary>Reject a single pending product with a reason.</summary>
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

        /// <summary>Request changes on a pending product (moves to RequestedChanges status).</summary>
        [HttpPost("products/request-changes")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RequestChanges(
            [FromBody] RejectProductDto dto)
        {
            var adminId = GetCurrentUserId();
            await _adminProductService.RequestChangesAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Changes requested. Seller has been notified."
            ));
        }

        /// <summary>Bulk-approve multiple pending products.</summary>
        [HttpPost("products/bulk-approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> BulkApprove(
            [FromBody] BulkProductActionDto dto)
        {
            var adminId = GetCurrentUserId();
            await _adminProductService.BulkApproveAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: $"Bulk approve completed for {dto.ProductIds.Count} product(s)."
            ));
        }

        /// <summary>Bulk-reject multiple pending products with a shared reason.</summary>
        [HttpPost("products/bulk-reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> BulkReject(
            [FromBody] BulkProductActionDto dto)
        {
            var adminId = GetCurrentUserId();
            await _adminProductService.BulkRejectAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: $"Bulk reject completed for {dto.ProductIds.Count} product(s)."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-10: Product Moderation
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get all products with rich filters for the moderation table.</summary>
        [HttpGet("products")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<AdminProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<AdminProductDto>>>> GetAllProducts(
            [FromQuery] AdminProductFilterDto filter)
        {
            var result = await _adminProductService.GetAllProductsAsync(filter);
            return Ok(ApiResponse<Pagination<AdminProductDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} product(s)"
            ));
        }

        /// <summary>Hide a product from the marketplace (soft visibility toggle).</summary>
        [HttpPost("products/{id:int}/hide")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> HideProduct(int id)
        {
            await _adminProductService.HideProductAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Product hidden from marketplace."));
        }

        /// <summary>Restore a hidden product back to visible.</summary>
        [HttpPost("products/{id:int}/restore")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RestoreProduct(int id)
        {
            await _adminProductService.RestoreProductAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Product restored to marketplace."));
        }

        /// <summary>Feature a product on the homepage.</summary>
        [HttpPost("products/{id:int}/feature")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> FeatureProduct(int id)
        {
            await _adminProductService.FeatureProductAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Product is now featured on the homepage."));
        }

        /// <summary>Remove a product from the homepage feature list.</summary>
        [HttpPost("products/{id:int}/unfeature")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> UnfeatureProduct(int id)
        {
            await _adminProductService.UnfeatureProductAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Product removed from homepage feature list."));
        }

        /// <summary>Re-assign a product to a different category.</summary>
        [HttpPost("products/change-category")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> ChangeProductCategory(
            [FromBody] ChangeProductCategoryDto dto)
        {
            await _adminProductService.ChangeProductCategoryAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Product category updated successfully."));
        }

        /// <summary>Get per-product analytics (views, add-to-cart, purchases, revenue, conversion rate).</summary>
        [HttpGet("products/{id:int}/analytics")]
        [ProducesResponseType(typeof(ApiResponse<AdminProductAnalyticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminProductAnalyticsDto>>> GetProductAnalytics(int id)
        {
            var result = await _adminProductService.GetProductAnalyticsAsync(id);
            return Ok(ApiResponse<AdminProductAnalyticsDto>.SuccessResponse(data: result));
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-11: Low Stock Alerts
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get paginated list of low-stock products (default threshold: 5 units).</summary>
        [HttpGet("products/low-stock")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<LowStockProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<LowStockProductDto>>>> GetLowStockProducts(
            [FromQuery] LowStockFilterDto filter)
        {
            var result = await _adminProductService.GetLowStockProductsAsync(filter);
            return Ok(ApiResponse<Pagination<LowStockProductDto>>.SuccessResponse(
                data: result,
                message: $"Found {result.Count} low-stock product(s)"
            ));
        }

        /// <summary>Manually notify the seller of a specific low-stock product.</summary>
        [HttpPost("products/{id:int}/notify-seller-stock")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> NotifySellerLowStock(int id)
        {
            await _adminProductService.NotifySellerLowStockAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Low-stock notification sent to seller."));
        }

        /// <summary>Send low-stock notifications to all sellers with at-threshold products.</summary>
        [HttpPost("products/notify-all-low-stock")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> BulkNotifyLowStock()
        {
            await _adminProductService.BulkNotifySellersLowStockAsync(GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(message: "Low-stock notifications sent to all affected sellers."));
        }
    }
}