using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.BoProductionRequest;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Business Owner endpoint for submitting and tracking production service requests to Talentree.
    /// </summary>
    [Authorize(Roles = "BusinessOwner")]
    public class BoProductionRequestController : BaseApiController
    {
        private readonly IBoProductionRequestService _service;

        public BoProductionRequestController(IBoProductionRequestService service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns a paginated list of the BO's own production requests, newest first.
        /// </summary>
        /// <remarks>GET /api/bo-production-requests?pageIndex=1&amp;pageSize=20</remarks>
        [HttpGet]
        public async Task<IActionResult> GetMyRequests(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetMyRequestsAsync(GetBoId(), pageIndex, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Returns the full detail of a single production request including status history.
        /// </summary>
        /// <remarks>GET /api/bo-production-requests/3</remarks>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetRequestByIdAsync(GetBoId(), id);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Submits a new production request to Talentree.
        /// </summary>
        /// <remarks>
        /// POST /api/bo-production-requests
        /// Body: {
        ///   "title": "Summer Collection",
        ///   "notes": "Need by end of April",
        ///   "items": [
        ///     { "productType": "Hoodie", "quantity": 100, "preferredRawMaterialId": 3, "specifications": "Black, size M" },
        ///     { "productType": "Crossbody Bag", "quantity": 200 }
        ///   ]
        /// }
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitProductionRequestDto dto)
        {
            var result = await _service.SubmitRequestAsync(GetBoId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<object>.SuccessResponse(result,
                    "Production request submitted. Talentree will review it and send you a quote."));
        }

        /// <summary>
        /// Accepts Talentree's price quote, authorising production to begin.
        /// Only allowed when the request is in Quoted status.
        /// </summary>
        /// <remarks>POST /api/bo-production-requests/3/confirm</remarks>
        [HttpPost("{id:int}/confirm")]
        public async Task<IActionResult> ConfirmQuote(int id)
        {
            var result = await _service.ConfirmQuoteAsync(GetBoId(), id);
            return Ok(ApiResponse<object>.SuccessResponse(result,
                "Quote confirmed. Talentree will start production shortly."));
        }

        /// <summary>
        /// Cancels the production request.
        /// Only allowed while the request is in Submitted or Quoted status.
        /// </summary>
        /// <remarks>DELETE /api/bo-production-requests/3</remarks>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _service.CancelRequestAsync(GetBoId(), id);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Production request cancelled."));
        }

        private string GetBoId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}