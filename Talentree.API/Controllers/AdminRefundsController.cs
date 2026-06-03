using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Refund;

namespace Talentree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminRefundsController : ControllerBase
    {
        private readonly IRefundService _refundService;

        public AdminRefundsController(IRefundService refundService)
        {
            _refundService = refundService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingRefunds([FromQuery] RefundStatus? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _refundService.GetPendingRefundsAsync(status, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRefund(int id)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _refundService.ApproveRefundAsync(id, adminId);
            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRefund(int id, [FromBody] RejectRefundDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _refundService.RejectRefundAsync(id, adminId, dto);
            return Ok(result);
        }
    }
}
