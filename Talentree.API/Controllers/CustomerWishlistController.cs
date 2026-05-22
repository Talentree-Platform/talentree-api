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
    public class CustomerWishlistController : BaseApiController
    {
        private readonly IWishlistService _wishlistService;

        public CustomerWishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        // GET /api/customer/wishlist
        [HttpGet("wishlist")]
        [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<WishlistDto>>> GetWishlist()
        {
            var userId = GetCurrentUserId();
            var wishlist = await _wishlistService.GetWishlistAsync(userId);
            return Ok(ApiResponse<WishlistDto>.SuccessResponse(wishlist, "Customer wishlist loaded successfully"));
        }

        // POST /api/customer/wishlist/{productId}
        [HttpPost("wishlist/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<WishlistDto>>> AddToWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            var wishlist = await _wishlistService.AddToWishlistAsync(productId, userId);
            return Ok(ApiResponse<WishlistDto>.SuccessResponse(wishlist, "Product added to wishlist successfully"));
        }

        // DELETE /api/customer/wishlist/{productId}
        [HttpDelete("wishlist/{productId:int}")]
        [ProducesResponseType(typeof(ApiResponse<WishlistDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<WishlistDto>>> RemoveFromWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            var wishlist = await _wishlistService.RemoveFromWishlistAsync(productId, userId);
            return Ok(ApiResponse<WishlistDto>.SuccessResponse(wishlist, "Product removed from wishlist successfully"));
        }

        // POST /api/customer/wishlist/move-to-cart
        [HttpPost("wishlist/move-to-cart")]
        [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CartDto>>> MoveAllToCart()
        {
            var userId = GetCurrentUserId();
            var cart = await _wishlistService.MoveAllToCartAsync(userId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "All available wishlist items moved to cart successfully"));
        }

        // GET /api/customer/wishlist/{productId}/status
        [HttpGet("wishlist/{productId:int}/status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> GetWishlistStatus(int productId)
        {
            var userId = GetCurrentUserId();
            var isWishlisted = await _wishlistService.IsWishlistedAsync(productId, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(isWishlisted, "Product wishlist status retrieved"));
        }
    }
}
