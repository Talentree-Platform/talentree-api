using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin.Orders;

namespace Talentree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderService _orderService;

        public AdminOrdersController(IAdminOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] AdminOrderFilterDto filter)
        {
            var result = await _orderService.GetOrdersAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetOrderStats()
        {
            var result = await _orderService.GetOrderStatsAsync();
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _orderService.UpdateOrderStatusAsync(id, dto, adminId);
            return Ok(result);
        }

        [HttpPost("{id}/notes")]
        public async Task<IActionResult> AddAdminNote(int id, [FromBody] string note)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _orderService.AddAdminNoteAsync(id, note, adminId);
            return Ok(result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportOrders([FromQuery] AdminOrderFilterDto filter)
        {
            var csvBytes = await _orderService.ExportOrdersToCsvAsync(filter);
            return File(csvBytes, "text/csv", $"orders_export_{System.DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}
