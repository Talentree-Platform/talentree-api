using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Customer;

namespace Talentree.API.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("api/customer")]
    public class CustomerCartController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CustomerCartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET /api/customer/cart
        [HttpGet("cart")]
        [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Shopping cart loaded successfully"));
        }

        // POST /api/customer/cart/items
        [HttpPost("cart/items")]
        [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.AddToCartAsync(dto, userId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Item added to cart successfully"));
        }

        // PUT /api/customer/cart/items/{productId}
        [HttpPut("cart/items/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem(int productId, [FromBody] UpdateCartItemDto dto)
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.UpdateCartItemAsync(productId, dto, userId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Cart item quantity updated successfully"));
        }

        // DELETE /api/customer/cart/items/{productId}
        [HttpDelete("cart/items/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CartDto>>> RemoveFromCart(int productId)
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.RemoveFromCartAsync(productId, userId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Item removed from cart successfully"));
        }

        // DELETE /api/customer/cart
        [HttpDelete("cart")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse("Shopping cart cleared successfully"));
        }

        // GET /api/customer/cart/checkout-preview
        [HttpGet("cart/checkout-preview")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CheckoutSummaryDto>>> PreviewCheckout([FromQuery] CheckoutDeliveryDto delivery)
        {
            var userId = GetCurrentUserId();
            var summary = await _cartService.PreviewCheckoutAsync(userId, delivery);
            return Ok(ApiResponse<CheckoutSummaryDto>.SuccessResponse(summary, "Checkout preview calculated successfully"));
        }
    }
}
