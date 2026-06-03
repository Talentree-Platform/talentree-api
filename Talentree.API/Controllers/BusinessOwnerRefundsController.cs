using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Refund;

namespace Talentree.API.Controllers
{
    [Route("api/business-owner/refund-requests")]
    [ApiController]
    [Authorize(Roles = "BusinessOwner")]
    public class BusinessOwnerRefundsController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public BusinessOwnerRefundsController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRefundRequests([FromQuery] RefundStatus? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var boId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _refundService.GetBoRefundRequestsAsync(boId, status, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpPost("{id}/respond")]
        public async Task<IActionResult> RespondToRefundRequest(int id, [FromBody] RespondRefundDto dto)
        {
            var boId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _refundService.RespondToRefundRequestAsync(id, boId, dto);
            return Ok(result);
        }
    }
}
