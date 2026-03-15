using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.MaterialOrder;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Handles checkout and order history for Business Owner raw-material purchases.
    /// </summary>
    [Authorize(Roles = "BusinessOwner")]
    public class MaterialOrderController : BaseApiController
    {
        private readonly IMaterialOrderService _orderService;

        public MaterialOrderController(IMaterialOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates a material order from the BO's current basket.
        /// Validates stock, locks prices, deducts inventory, and clears the basket
        /// in a single transaction.
        /// </summary>
        /// <remarks>
        /// POST /api/material-orders/checkout
        /// Body: { "deliveryAddress": "...", "deliveryCity": "...", "deliveryCountry": "...",
        ///         "contactPhone": "...", "paymentMethod": "CashOnDelivery" }
        /// </remarks>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] MaterialCheckoutDto dto)
        {
            var result = await _orderService.CheckoutAsync(GetBoId(), dto);
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Id },
                ApiResponse<object>.SuccessResponse(result, "Order placed successfully."));
        }

        /// <summary>
        /// Returns a paginated list of the BO's material order history, newest first.
        /// </summary>
        /// <remarks>GET /api/material-orders?pageIndex=1&amp;pageSize=20</remarks>
        [HttpGet]
        public async Task<IActionResult> GetOrderHistory(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _orderService.GetOrderHistoryAsync(GetBoId(), pageIndex, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Returns the full detail of a single material order including all line items.
        /// </summary>
        /// <remarks>GET /api/material-orders/12</remarks>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(GetBoId(), id);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        private string GetBoId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}