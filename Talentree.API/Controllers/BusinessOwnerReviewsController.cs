using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Reviews;

namespace Talentree.API.Controllers
{
    [Authorize(Roles = "BusinessOwner")]
    public class BusinessOwnerReviewsController : BaseApiController
    {
        private readonly IReviewService _reviewService;

        public BusinessOwnerReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ═══════════════════════════════════════════════════════════
        // GET: api/business-owner-reviews
        // FR-BO-24: View all reviews with filter/sort/search
        // ═══════════════════════════════════════════════════════════
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Pagination<ReviewDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<ReviewDto>>>> GetMyReviews(
            [FromQuery] ReviewFilterDto filter)
        {
            var result = await _reviewService.GetMyReviewsAsync(GetUserId(), filter);

            return Ok(ApiResponse<Pagination<ReviewDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} reviews " +
                         $"(Page {result.PageIndex} of {result.TotalPages})"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // POST: api/business-owner-reviews/{id}/respond
        // FR-BO-25: Respond to a review
        // ═══════════════════════════════════════════════════════════
        [HttpPost("{id:int}/respond")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> RespondToReview(
            int id,
            [FromBody] RespondToReviewDto dto)
        {
            var result = await _reviewService.RespondToReviewAsync(id, dto, GetUserId());

            return Ok(ApiResponse<ReviewDto>.SuccessResponse(
                data: result,
                message: "Response posted successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // PUT: api/business-owner-reviews/{id}/respond
        // FR-BO-25: Edit response (within 24 hours)
        // ═══════════════════════════════════════════════════════════
        [HttpPut("{id:int}/respond")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> EditResponse(
            int id,
            [FromBody] RespondToReviewDto dto)
        {
            var result = await _reviewService.EditResponseAsync(id, dto, GetUserId());

            return Ok(ApiResponse<ReviewDto>.SuccessResponse(
                data: result,
                message: "Response updated successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/business-owner-reviews/analytics
        // FR-BO-26: Review analytics
        // ═══════════════════════════════════════════════════════════
        [HttpGet("analytics")]
        [ProducesResponseType(typeof(ApiResponse<ReviewAnalyticsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ReviewAnalyticsDto>>> GetAnalytics()
        {
            var result = await _reviewService.GetReviewAnalyticsAsync(GetUserId());

            return Ok(ApiResponse<ReviewAnalyticsDto>.SuccessResponse(
                data: result,
                message: "Review analytics retrieved successfully"
            ));
        }
    }
}