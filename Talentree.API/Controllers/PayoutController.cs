using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Payout;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Business Owner payout requests and Admin payout processing.
    /// </summary>
    public class PayoutController : BaseApiController
    {
        private readonly IPayoutService _payoutService;

        public PayoutController(IPayoutService payoutService)
            => _payoutService = payoutService;

        // ══════════════════════════════════════════════════════
        // BO endpoints
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// Submits a payout request.
        /// Fails if the BO already has a pending request or the amount exceeds available balance.
        /// </summary>
        /// <remarks>POST /api/payout</remarks>
        [HttpPost]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> RequestPayout([FromBody] CreatePayoutRequestDto dto)
        {
            var result = await _payoutService.CreateRequestAsync(GetUserId(), dto);
            return Ok(ApiResponse<object>.SuccessResponse(result,
                "Payout request submitted. Processing takes 3–5 business days."));
        }

        /// <summary>Returns the BO's payout request history.</summary>
        /// <remarks>GET /api/payout/my-history?pageIndex=1&amp;pageSize=20</remarks>
        [HttpGet("my-history")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> GetMyHistory(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _payoutService.GetBoHistoryAsync(GetUserId(), pageIndex, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        // ══════════════════════════════════════════════════════
        // Admin endpoints
        // ══════════════════════════════════════════════════════

        /// <summary>Returns all payout requests, optionally filtered by status.</summary>
        /// <remarks>GET /api/payout/admin?status=Pending</remarks>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] PayoutStatus? status,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _payoutService.GetAllAsync(status, pageIndex, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>Approves a payout request.</summary>
        /// <remarks>PUT /api/payout/admin/5/approve</remarks>
        [HttpPut("admin/{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _payoutService.ApproveAsync(id, GetUserId());
            return Ok(ApiResponse<object>.SuccessResponse(result, "Payout request approved."));
        }

        /// <summary>Marks a payout request as completed (funds transferred).</summary>
        /// <remarks>PUT /api/payout/admin/5/complete</remarks>
        [HttpPut("admin/{id:int}/complete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Complete(int id)
        {
            var result = await _payoutService.CompleteAsync(id, GetUserId());
            return Ok(ApiResponse<object>.SuccessResponse(result, "Payout marked as completed."));
        }

        /// <summary>Rejects a payout request with a mandatory reason.</summary>
        /// <remarks>PUT /api/payout/admin/5/reject</remarks>
        [HttpPut("admin/{id:int}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectPayoutDto dto)
        {
            var result = await _payoutService.RejectAsync(id, GetUserId(), dto);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Payout request rejected."));
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}