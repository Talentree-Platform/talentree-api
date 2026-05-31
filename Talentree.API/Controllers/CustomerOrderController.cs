using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Customer;

namespace Talentree.API.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("api/customer")]
    public class CustomerOrderController : BaseApiController
    {
        private readonly ICustomerOrderService _orderService;

        public CustomerOrderController(ICustomerOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST /api/customer/orders
        [HttpPost("orders")]
        [ProducesResponseType(typeof(ApiResponse<CustomerOrderDetailDto>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<CustomerOrderDetailDto>>> PlaceOrder([FromBody] CheckoutDeliveryDto delivery, [FromQuery] PaymentMethod method)
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.PlaceOrderAsync(delivery, method, userId);
            
            return CreatedAtAction(
                nameof(GetOrder), 
                new { id = order.Id }, 
                ApiResponse<CustomerOrderDetailDto>.SuccessResponse(order, "Order placed successfully")
            );
        }

        // POST /api/customer/orders/{id}/payment-intent
        [HttpPost("orders/{id:int}/payment-intent")]
        [ProducesResponseType(typeof(ApiResponse<OrderPaymentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<OrderPaymentDto>>> CreatePaymentIntent(int id)
        {
            var userId = GetCurrentUserId();
            var paymentDto = await _orderService.CreatePaymentIntentAsync(id, userId);
            return Ok(ApiResponse<OrderPaymentDto>.SuccessResponse(paymentDto, "Payment intent created successfully"));
        }

        // GET /api/customer/orders
        [HttpGet("orders")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<CustomerOrderSummaryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<CustomerOrderSummaryDto>>>> GetOrders([FromQuery] OrderFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var orders = await _orderService.GetOrdersAsync(userId, filter);
            return Ok(ApiResponse<Pagination<CustomerOrderSummaryDto>>.SuccessResponse(orders, "Orders history loaded successfully"));
        }

        // GET /api/customer/orders/{id}
        [HttpGet("orders/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CustomerOrderDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CustomerOrderDetailDto>>> GetOrder(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.GetOrderByIdAsync(id, userId);
            return Ok(ApiResponse<CustomerOrderDetailDto>.SuccessResponse(order, "Order details loaded successfully"));
        }

        // POST /api/customer/orders/{id}/cancel
        [HttpPost("orders/{id:int}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> CancelOrder(int id)
        {
            var userId = GetCurrentUserId();
            await _orderService.CancelOrderAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse("Order cancelled successfully"));
        }

        // GET /api/customer/orders/{id}/invoice
        [HttpGet("orders/{id:int}/invoice")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var userId = GetCurrentUserId();
            var pdfBytes = await _orderService.GenerateInvoiceAsync(id, userId);
            return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
        }
    }
}
