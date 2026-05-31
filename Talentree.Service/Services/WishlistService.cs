using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.WishlistSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;

        public WishlistService(IUnitOfWork unitOfWork, IMapper mapper, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cartService = cartService;
        }

        private async Task<CustomerWishlist> GetOrCreateWishlistEntityAsync(string customerId)
        {
            var spec = new WishlistByCustomerSpecification(customerId);
            var wishlists = await _unitOfWork.Repository<CustomerWishlist>().GetAllWithSpecificationsAsync(spec);
            var wishlist = wishlists.FirstOrDefault();

            if (wishlist == null)
            {
                wishlist = new CustomerWishlist
                {
                    CustomerId = customerId,
                    Items = new List<CustomerWishlistItem>()
                };
                _unitOfWork.Repository<CustomerWishlist>().Add(wishlist);
                await _unitOfWork.CompleteAsync();

                // Reload with specifications so navigation properties are populated
                var reloaded = await _unitOfWork.Repository<CustomerWishlist>().GetAllWithSpecificationsAsync(spec);
                wishlist = reloaded.First();
            }

            return wishlist;
        }

        public async Task<WishlistDto> GetWishlistAsync(string customerId)
        {
            var wishlist = await GetOrCreateWishlistEntityAsync(customerId);
            return _mapper.Map<WishlistDto>(wishlist);
        }

        public async Task<WishlistDto> AddToWishlistAsync(int productId, string customerId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with ID {productId} not found.");

            var wishlist = await GetOrCreateWishlistEntityAsync(customerId);
            var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem == null)
            {
                var newItem = new CustomerWishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = productId,
                    AddedAt = DateTime.UtcNow
                };
                _unitOfWork.Repository<CustomerWishlistItem>().Add(newItem);
                await _unitOfWork.CompleteAsync();
            }

            // Refresh from database
            var spec = new WishlistByCustomerSpecification(customerId);
            var refreshed = await _unitOfWork.Repository<CustomerWishlist>().GetAllWithSpecificationsAsync(spec);
            return _mapper.Map<WishlistDto>(refreshed.First());
        }

        public async Task<WishlistDto> RemoveFromWishlistAsync(int productId, string customerId)
        {
            var wishlist = await GetOrCreateWishlistEntityAsync(customerId);
            var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem == null)
                throw new NotFoundException("Product not found in your wishlist.");

            _unitOfWork.Repository<CustomerWishlistItem>().Delete(existingItem);
            await _unitOfWork.CompleteAsync();

            // Refresh from database
            var spec = new WishlistByCustomerSpecification(customerId);
            var refreshed = await _unitOfWork.Repository<CustomerWishlist>().GetAllWithSpecificationsAsync(spec);
            return _mapper.Map<WishlistDto>(refreshed.First());
        }

        public async Task<CartDto> MoveAllToCartAsync(string customerId)
        {
            var wishlist = await GetOrCreateWishlistEntityAsync(customerId);
            
            foreach (var item in wishlist.Items.ToList())
            {
                if (item.Product != null && item.Product.StockQuantity > 0)
                {
                    try
                    {
                        // Add 1 of the item to cart
                        await _cartService.AddToCartAsync(new AddToCartDto { ProductId = item.ProductId, Quantity = 1 }, customerId);
                        
                        // Remove from wishlist on success
                        _unitOfWork.Repository<CustomerWishlistItem>().Delete(item);
                    }
                    catch (BadRequestException)
                    {
                        // E.g. If already in cart and adding 1 exceeds stock, skip moving
                    }
                }
            }

            await _unitOfWork.CompleteAsync();

            return await _cartService.GetCartAsync(customerId);
        }

        public async Task<bool> IsWishlistedAsync(int productId, string customerId)
        {
            var spec = new WishlistByCustomerSpecification(customerId);
            var wishlists = await _unitOfWork.Repository<CustomerWishlist>().GetAllWithSpecificationsAsync(spec);
            var wishlist = wishlists.FirstOrDefault();

            if (wishlist == null)
                return false;

            return wishlist.Items.Any(i => i.ProductId == productId);
        }
    }
}
