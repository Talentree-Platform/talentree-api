using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Basket;
using Talentree.Core.Entities;
using Talentree.Core.Service.Contract;
using Talentree.Repository.Data;

namespace Talentree.Service.Services
{
    public class MaterialBasketService : IMaterialBasketService
    {
        private readonly TalentreeDbContext _context;

        public MaterialBasketService(TalentreeDbContext context)
        {
            _context = context;
        }

        public async Task<MaterialBasketDto> GetBasketAsync(string businessOwnerId)
        {
            var basket = await GetOrCreateBasketAsync(businessOwnerId);
            var loaded = await LoadBasketWithDetailsAsync(basket.Id);
            return MapToDto(loaded);
        }

        public async Task<MaterialBasketDto> AddItemAsync(string businessOwnerId, AddToBasketDto dto)
        {
            var material = await _context.Set<RawMaterial>()
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == dto.RawMaterialId && m.IsAvailable)
                ?? throw new KeyNotFoundException("Material not found or unavailable.");

            if (dto.Quantity < material.MinimumOrderQuantity)
                throw new InvalidOperationException(
                    $"Minimum order quantity for '{material.Name}' is {material.MinimumOrderQuantity} {material.Unit}.");

            if (dto.Quantity > material.StockQuantity)
                throw new InvalidOperationException(
                    $"Only {material.StockQuantity} {material.Unit} of '{material.Name}' available in stock.");

            var basket = await GetOrCreateBasketAsync(businessOwnerId);

            // If already in basket, increment quantity instead of adding duplicate
            var existing = basket.Items.FirstOrDefault(i => i.RawMaterialId == dto.RawMaterialId);
            if (existing != null)
            {
                var newQty = existing.Quantity + dto.Quantity;
                if (newQty > material.StockQuantity)
                    throw new InvalidOperationException(
                        $"Cannot exceed available stock. You already have {existing.Quantity} in your basket.");
                existing.Quantity = newQty;
            }
            else
            {
                basket.Items.Add(new MaterialBasketItem
                {
                    BasketId = basket.Id,
                    RawMaterialId = dto.RawMaterialId,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();

            var loaded = await LoadBasketWithDetailsAsync(basket.Id);
            return MapToDto(loaded);
        }

        public async Task<MaterialBasketDto> UpdateItemAsync(string businessOwnerId, int itemId, UpdateBasketItemDto dto)
        {
            var basket = await LoadBasketByOwnerAsync(businessOwnerId);

            var item = basket.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException("Basket item not found.");

            var material = await _context.Set<RawMaterial>().FindAsync(item.RawMaterialId)
                ?? throw new KeyNotFoundException("Material no longer exists.");

            if (dto.Quantity < material.MinimumOrderQuantity)
                throw new InvalidOperationException(
                    $"Minimum order quantity is {material.MinimumOrderQuantity} {material.Unit}.");

            if (dto.Quantity > material.StockQuantity)
                throw new InvalidOperationException(
                    $"Only {material.StockQuantity} {material.Unit} available in stock.");

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            var loaded = await LoadBasketWithDetailsAsync(basket.Id);
            return MapToDto(loaded);
        }

        public async Task<MaterialBasketDto> RemoveItemAsync(string businessOwnerId, int itemId)
        {
            var basket = await LoadBasketByOwnerAsync(businessOwnerId);

            var item = basket.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException("Basket item not found.");

            _context.Set<MaterialBasketItem>().Remove(item);
            await _context.SaveChangesAsync();

            var loaded = await LoadBasketWithDetailsAsync(basket.Id);
            return MapToDto(loaded);
        }

        public async Task ClearBasketAsync(string businessOwnerId)
        {
            var basket = await LoadBasketByOwnerAsync(businessOwnerId);
            _context.Set<MaterialBasketItem>().RemoveRange(basket.Items);
            await _context.SaveChangesAsync();
        }

        // ── Private helpers ───────────────────────────────────────

        private async Task<MaterialBasket> GetOrCreateBasketAsync(string businessOwnerId)
        {
            var basket = await _context.Set<MaterialBasket>()
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.BusinessOwnerId == businessOwnerId);

            if (basket != null) return basket;

            // First time — create an empty basket for this BO
            var newBasket = new MaterialBasket { BusinessOwnerId = businessOwnerId };
            _context.Set<MaterialBasket>().Add(newBasket);
            await _context.SaveChangesAsync();
            return newBasket;
        }

        private async Task<MaterialBasket> LoadBasketByOwnerAsync(string businessOwnerId)
            => await _context.Set<MaterialBasket>()
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.BusinessOwnerId == businessOwnerId)
               ?? throw new KeyNotFoundException("Basket not found.");

        // Loads basket with full navigation properties for DTO mapping
        private Task<MaterialBasket> LoadBasketWithDetailsAsync(int basketId)
            => _context.Set<MaterialBasket>()
                .Include(b => b.Items)
                    .ThenInclude(i => i.RawMaterial)
                        .ThenInclude(m => m.Supplier)
                .FirstAsync(b => b.Id == basketId);

        private static MaterialBasketDto MapToDto(MaterialBasket basket) => new()
        {
            Id = basket.Id,
            Items = basket.Items.Select(i => new MaterialBasketItemDto
            {
                Id = i.Id,
                RawMaterialId = i.RawMaterialId,
                MaterialName = i.RawMaterial?.Name ?? string.Empty,
                PictureUrl = i.RawMaterial?.PictureUrl,
                Unit = i.RawMaterial?.Unit ?? string.Empty,
                SupplierName = i.RawMaterial?.Supplier?.Name ?? string.Empty,
                UnitPrice = i.RawMaterial?.Price ?? 0,
                Quantity = i.Quantity,
                MinimumOrderQuantity = i.RawMaterial?.MinimumOrderQuantity ?? 1
            }).ToList()
        };
    }
}
