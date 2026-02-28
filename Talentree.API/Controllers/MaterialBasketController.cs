using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.DTOs.Basket;
using Talentree.Service.Contracts;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Manages the Business Owner's raw material shopping basket.
    /// The basket persists across sessions and is shared between
    /// standalone material purchases and production order flows.
    /// </summary>
    [Authorize(Roles = "BusinessOwner")]
    public class MaterialBasketController : BaseApiController
    {
        private readonly IMaterialBasketService _basketService;

        public MaterialBasketController(IMaterialBasketService basketService)
        {
            _basketService = basketService;
        }

        /// <summary>
        /// Gets the current Business Owner's basket.
        /// Creates an empty basket automatically if one doesn't exist yet.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<MaterialBasketDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBasket()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var basket = await _basketService.GetBasketAsync(userId);

            return Ok(ApiResponse<MaterialBasketDto>.SuccessResponse(
                data: basket,
                message: $"Basket retrieved. {basket.TotalItemCount} item(s), subtotal: {basket.Subtotal:C}"
            ));
        }

        /// <summary>
        /// Adds a raw material to the basket.
        /// Validates minimum order quantity and available stock.
        /// If the material is already in the basket, the quantity is incremented.
        /// </summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponse<MaterialBasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddItem([FromBody] AddToBasketDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var basket = await _basketService.AddItemAsync(userId, dto);

            return Ok(ApiResponse<MaterialBasketDto>.SuccessResponse(
                data: basket,
                message: "Item added to basket."
            ));
        }

        /// <summary>
        /// Updates the quantity of an existing basket item.
        /// Validates the new quantity against minimum order quantity and available stock.
        /// </summary>
        /// <param name="itemId">The basket item ID to update</param>
        [HttpPut("items/{itemId:int}")]
        [ProducesResponseType(typeof(ApiResponse<MaterialBasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateBasketItemDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var basket = await _basketService.UpdateItemAsync(userId, itemId, dto);

            return Ok(ApiResponse<MaterialBasketDto>.SuccessResponse(
                data: basket,
                message: "Basket item updated."
            ));
        }

        /// <summary>
        /// Removes a single item from the basket.
        /// </summary>
        /// <param name="itemId">The basket item ID to remove</param>
        [HttpDelete("items/{itemId:int}")]
        [ProducesResponseType(typeof(ApiResponse<MaterialBasketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var basket = await _basketService.RemoveItemAsync(userId, itemId);

            return Ok(ApiResponse<MaterialBasketDto>.SuccessResponse(
                data: basket,
                message: "Item removed from basket."
            ));
        }

        /// <summary>
        /// Removes all items from the basket.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearBasket()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _basketService.ClearBasketAsync(userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Basket cleared."
            ));
        }
    }
}
