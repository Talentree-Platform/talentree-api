using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.CartSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ILogger<CartService> _logger;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper,
            IUserInteractionService userInteractionService,
            ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userInteractionService = userInteractionService;
            _logger = logger;
        }

        private async Task<CustomerCart> GetOrCreateCartEntityAsync(string customerId)
        {
            var spec = new CartByCustomerSpecification(customerId);
            var carts = await _unitOfWork.Repository<CustomerCart>().GetAllWithSpecificationsAsync(spec);
            var cart = carts.FirstOrDefault();

            if (cart == null)
            {
                cart = new CustomerCart
                {
                    CustomerId = customerId,
                    Items = new List<CustomerCartItem>()
                };
                _unitOfWork.Repository<CustomerCart>().Add(cart);
                await _unitOfWork.CompleteAsync();
                
                // Reload with specifications so navigation properties are populated
                var reloadedCarts = await _unitOfWork.Repository<CustomerCart>().GetAllWithSpecificationsAsync(spec);
                cart = reloadedCarts.First();
            }

            return cart;
        }

        public async Task<CartDto> GetCartAsync(string customerId)
        {
            var cart = await GetOrCreateCartEntityAsync(customerId);
            return MapToCartDto(cart);
        }

        public async Task<CartDto> AddToCartAsync(AddToCartDto dto, string customerId)
        {
            if (dto.Quantity <= 0)
                throw new BadRequestException("Quantity must be greater than zero");

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new NotFoundException($"Product with ID {dto.ProductId} not found");

            if (product.StockQuantity < dto.Quantity)
                throw new BadRequestException($"Insufficient stock. Only {product.StockQuantity} item(s) available");

            var cart = await GetOrCreateCartEntityAsync(customerId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                var newQty = existingItem.Quantity + dto.Quantity;
                if (product.StockQuantity < newQty)
                    throw new BadRequestException($"Cannot add {dto.Quantity} more item(s). Total in cart ({newQty}) exceeds stock ({product.StockQuantity})");

                existingItem.Quantity = newQty;
                _unitOfWork.Repository<CustomerCartItem>().Update(existingItem);
            }
            else
            {
                var newItem = new CustomerCartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    AddedAt = DateTime.UtcNow
                };
                _unitOfWork.Repository<CustomerCartItem>().Add(newItem);
            }

            await _unitOfWork.CompleteAsync();

            // ✅ SAVE product data BEFORE Task.Run (before DbContext dispose)
            var productData = new
            {
                product.Id,
                product.Price,
                CategoryName = product.Category?.Name ?? "Uncategorized"
            };

            // Refresh the cart from db with specs
            var spec = new CartByCustomerSpecification(customerId);
            var refreshedCarts = await _unitOfWork.Repository<CustomerCart>()
                .GetAllWithSpecificationsAsync(spec);

            // ✅ Log click interaction (fire-and-forget)
            if (!string.IsNullOrEmpty(customerId))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _userInteractionService.LogInteractionAsync(
                            userId: customerId,
                            userType: UserInteractionType.Customer,
                            itemId: productData.Id,
                            itemType: UserInteractionItemType.Product,
                            actionType: UserInteractionActionType.Click,
                            category: productData.CategoryName,  // ✅ use saved data
                            quantity: dto.Quantity,
                            price: productData.Price  // ✅ use saved data
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error logging interaction in AddToCart");
                    }
                });
            }

            return MapToCartDto(refreshedCarts.First());
        }
        public async Task<CartDto> UpdateCartItemAsync(int productId, UpdateCartItemDto dto, string customerId)
        {
            if (dto.Quantity <= 0)
                throw new BadRequestException("Quantity must be greater than zero");

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with ID {productId} not found");

            if (product.StockQuantity < dto.Quantity)
                throw new BadRequestException($"Insufficient stock. Only {product.StockQuantity} item(s) available");

            var cart = await GetOrCreateCartEntityAsync(customerId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem == null)
                throw new NotFoundException("Item not found in cart");

            existingItem.Quantity = dto.Quantity;
            _unitOfWork.Repository<CustomerCartItem>().Update(existingItem);
            await _unitOfWork.CompleteAsync();

            // Refresh the cart from db with specs
            var spec = new CartByCustomerSpecification(customerId);
            var refreshedCarts = await _unitOfWork.Repository<CustomerCart>().GetAllWithSpecificationsAsync(spec);
            return MapToCartDto(refreshedCarts.First());
        }

        public async Task<CartDto> RemoveFromCartAsync(int productId, string customerId)
        {
            var cart = await GetOrCreateCartEntityAsync(customerId);
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem == null)
                throw new NotFoundException("Item not found in cart");

            _unitOfWork.Repository<CustomerCartItem>().Delete(existingItem);
            await _unitOfWork.CompleteAsync();

            // Refresh the cart from db with specs
            var spec = new CartByCustomerSpecification(customerId);
            var refreshedCarts = await _unitOfWork.Repository<CustomerCart>().GetAllWithSpecificationsAsync(spec);
            return MapToCartDto(refreshedCarts.First());
        }

        public async Task ClearCartAsync(string customerId)
        {
            var cart = await GetOrCreateCartEntityAsync(customerId);
            if (cart.Items.Any())
            {
                foreach (var item in cart.Items.ToList())
                {
                    _unitOfWork.Repository<CustomerCartItem>().Delete(item);
                }
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<CheckoutSummaryDto> PreviewCheckoutAsync(string customerId, CheckoutDeliveryDto delivery)
        {
            var cart = await GetOrCreateCartEntityAsync(customerId);
            if (!cart.Items.Any())
                throw new BadRequestException("Cannot preview checkout for an empty cart");

            // Real-time stock validation
            foreach (var item in cart.Items)
            {
                if (item.Product.StockQuantity < item.Quantity)
                    throw new BadRequestException($"Stock levels for product '{item.Product.Name}' have changed. Only {item.Product.StockQuantity} available, but you have {item.Quantity} in your cart. Please adjust quantity.");
            }

            var cartDto = MapToCartDto(cart);

            return new CheckoutSummaryDto
            {
                Items = cartDto.Items,
                Subtotal = cartDto.Subtotal,
                ShippingAmount = cartDto.EstimatedShipping,
                Total = cartDto.Total,
                Delivery = delivery
            };
        }

        private CartDto MapToCartDto(CustomerCart cart)
        {
            var dto = _mapper.Map<CartDto>(cart);
            dto.Items = _mapper.Map<List<CartItemDto>>(cart.Items);

            // Compute financials
            dto.Subtotal = dto.Items.Sum(i => i.LineTotal);
            // $10 shipping if under $100, else free
            dto.EstimatedShipping = dto.Subtotal > 0 && dto.Subtotal < 100m ? 10.00m : 0.00m;
            dto.Total = dto.Subtotal + dto.EstimatedShipping;

            return dto;
        }
    }
}
