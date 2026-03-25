using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.BoProductionRequest;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Admin endpoint for managing Business Owner production service requests.
    /// Controls the full Talentree workflow from review through to completion.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminProductionRequestController : BaseApiController
    {
        private readonly IAdminProductionRequestService _service;

        public AdminProductionRequestController(IAdminProductionRequestService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns all production requests across all BOs, optionally filtered by status.
        /// </summary>
        /// <remarks>
        /// GET /api/admin/production-requests?status=Submitted&amp;pageIndex=1&amp;pageSize=20
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] BoProductionRequestStatus? status,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetAllRequestsAsync(status, pageIndex, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Returns the full detail of any production request by ID.
        /// </summary>
        /// <remarks>GET /api/admin/production-requests/3</remarks>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetRequestByIdAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Marks the request as UnderReview.
        /// Only allowed from Submitted status.
        /// </summary>
        /// <remarks>PUT /api/admin/production-requests/3/review</remarks>
        [HttpPut("{id:int}/review")]
        public async Task<IActionResult> MarkUnderReview(int id)
        {
            var result = await _service.MarkUnderReviewAsync(id, GetAdminId());
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Sends a price quote and estimated timeline to the BO.
        /// Only allowed from UnderReview status.
        /// </summary>
        /// <remarks>
        /// PUT /api/admin/production-requests/3/quote
        /// Body: { "quotedPrice": 4500.00, "estimatedCompletionDate": "2026-05-01", "adminNotes": "..." }
        /// </remarks>
        [HttpPut("{id:int}/quote")]
        public async Task<IActionResult> SendQuote(int id, [FromBody] SendQuoteDto dto)
        {
            var result = await _service.SendQuoteAsync(id, GetAdminId(), dto);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Quote sent to business owner."));
        }

        /// <summary>
        /// Starts production on a confirmed request.
        /// Only allowed from Confirmed status (BO has already accepted the quote).
        /// </summary>
        /// <remarks>PUT /api/admin/production-requests/3/start</remarks>
        [HttpPut("{id:int}/start")]
        public async Task<IActionResult> StartProduction(int id)
        {
            var result = await _service.StartProductionAsync(id, GetAdminId());
            return Ok(ApiResponse<object>.SuccessResponse(result, "Production started."));
        }

        /// <summary>
        /// Marks production as completed — goods are ready for the Business Owner.
        /// Only allowed from InProduction status.
        /// </summary>
        /// <remarks>
        /// PUT /api/admin/production-requests/3/complete
        /// Body: { "adminNotes": "Ready for collection from warehouse B." }
        /// </remarks>
        [HttpPut("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id, [FromBody] CompleteRequestDto dto)
        {
            var result = await _service.CompleteRequestAsync(id, GetAdminId(), dto);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Production completed."));
        }

        /// <summary>
        /// Rejects the production request with a mandatory reason.
        /// Not allowed after InProduction has started.
        /// </summary>
        /// <remarks>
        /// PUT /api/admin/production-requests/3/reject
        /// Body: { "reason": "Requested material is out of stock until Q3." }
        /// </remarks>
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectRequestDto dto)
        {
            var result = await _service.RejectRequestAsync(id, GetAdminId(), dto);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Request rejected."));
        }

        private string GetAdminId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}