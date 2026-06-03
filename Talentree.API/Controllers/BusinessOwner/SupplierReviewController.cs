using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Supplier;
using Talentree.API.Models;

namespace Talentree.API.Controllers.BusinessOwner
{
    [Route("api/business-owner/suppliers")]
    [ApiController]
    [Authorize(Roles = "BusinessOwner")]
    public class SupplierReviewController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierReviewController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpPost("{supplierId}/reviews")]
        public async Task<IActionResult> AddReview(int supplierId, [FromBody] CreateSupplierReviewDto dto)
        {
            var boId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(boId))
                return Unauthorized();

            var review = await _supplierService.AddSupplierReviewAsync(supplierId, boId, dto);
            return Ok(ApiResponse<SupplierReviewDto>.SuccessResponse(review, "Review added successfully."));
        }

        [HttpGet("{supplierId}/reviews")]
        [AllowAnonymous] // Allow anyone to read reviews? Or only BOs? The requirement didn't specify. Let's keep it open to logged-in users or BOs.
        public async Task<IActionResult> GetReviews(
            int supplierId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var reviews = await _supplierService.GetSupplierReviewsAsync(supplierId, pageIndex, pageSize);
            return Ok(ApiResponse<Pagination<SupplierReviewDto>>.SuccessResponse(reviews));
        }
    }
}
