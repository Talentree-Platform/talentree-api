using AutoMapper;
using Talentree.Core;
using Talentree.Core.DTOs.Basket;
using Talentree.Core.Entities;
using Talentree.Core.Repository.Contract;
using Talentree.Core.Service.Contract;
using Talentree.Core.Specifications.Basket;
using Talentree.Core.Specifications.RawMaterial;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Manages the Business Owner's raw material shopping basket.
    /// The basket persists across sessions and is shared between
    /// standalone material purchases and production order flows.
    /// </summary>
    public class MaterialBasketService : IMaterialBasketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MaterialBasketService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<MaterialBasketDto> GetBasketAsync(string businessOwnerId)
        {
            var basket = await GetOrCreateBasketAsync(businessOwnerId);
            return _mapper.Map<MaterialBasketDto>(basket);
        }

        /// <inheritdoc/>
        public async Task<MaterialBasketDto> AddItemAsync(string businessOwnerId, AddToBasketDto dto)
        {
            var materialSpec = new RawMaterialByIdWithSupplierSpec(dto.RawMaterialId);
            var material = await _unitOfWork.Repository<RawMaterial>()
                .GetByIdWithSpecificationsAsync(materialSpec)
                ?? throw new KeyNotFoundException("Material not found or unavailable.");

            if (dto.Quantity < material.MinimumOrderQuantity)
                throw new InvalidOperationException(
                    $"Minimum order quantity for '{material.Name}' is {material.MinimumOrderQuantity} {material.Unit}.");

            if (dto.Quantity > material.StockQuantity)
                throw new InvalidOperationException(
                    $"Only {material.StockQuantity} {material.Unit} of '{material.Name}' available in stock.");

            var basket = await GetOrCreateBasketAsync(businessOwnerId);

            // If already in basket, increment quantity instead of adding a duplicate
            var existing = basket.Items.FirstOrDefault(i => i.RawMaterialId == dto.RawMaterialId);
            if (existing != null)
            {
                var newQty = existing.Quantity + dto.Quantity;
                if (newQty > material.StockQuantity)
                    throw new InvalidOperationException(
                        $"Cannot exceed available stock. You already have {existing.Quantity} in your basket.");
                existing.Quantity = newQty;
                _unitOfWork.Repository<MaterialBasketItem>().Update(existing);
            }
            else
            {
                var item = new MaterialBasketItem
                {
                    BasketId = basket.Id,
                    RawMaterialId = dto.RawMaterialId,
                    Quantity = dto.Quantity
                };
                _unitOfWork.Repository<MaterialBasketItem>().Add(item);
            }

            await _unitOfWork.CompleteAsync();

            // Reload basket with full navigation for accurate DTO
            var updated = await GetBasketWithDetailsAsync(businessOwnerId);
            return _mapper.Map<MaterialBasketDto>(updated);
        }

        /// <inheritdoc/>
        public async Task<MaterialBasketDto> UpdateItemAsync(string businessOwnerId, int itemId, UpdateBasketItemDto dto)
        {
            var basket = await GetBasketWithDetailsAsync(businessOwnerId)
                ?? throw new KeyNotFoundException("Basket not found.");

            var item = basket.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException("Basket item not found.");

            var material = await _unitOfWork.Repository<RawMaterial>().GetByIdAsync(item.RawMaterialId)
                ?? throw new KeyNotFoundException("Material no longer exists.");

            if (dto.Quantity < material.MinimumOrderQuantity)
                throw new InvalidOperationException(
                    $"Minimum order quantity is {material.MinimumOrderQuantity} {material.Unit}.");

            if (dto.Quantity > material.StockQuantity)
                throw new InvalidOperationException(
                    $"Only {material.StockQuantity} {material.Unit} available in stock.");

            item.Quantity = dto.Quantity;
            _unitOfWork.Repository<MaterialBasketItem>().Update(item);
            await _unitOfWork.CompleteAsync();

            var updated = await GetBasketWithDetailsAsync(businessOwnerId);
            return _mapper.Map<MaterialBasketDto>(updated);
        }

        /// <inheritdoc/>
        public async Task<MaterialBasketDto> RemoveItemAsync(string businessOwnerId, int itemId)
        {
            var basket = await GetBasketWithDetailsAsync(businessOwnerId)
                ?? throw new KeyNotFoundException("Basket not found.");

            var item = basket.Items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new KeyNotFoundException("Basket item not found.");

            _unitOfWork.Repository<MaterialBasketItem>().Delete(item);
            await _unitOfWork.CompleteAsync();

            var updated = await GetBasketWithDetailsAsync(businessOwnerId);
            return _mapper.Map<MaterialBasketDto>(updated);
        }

        /// <inheritdoc/>
        public async Task ClearBasketAsync(string businessOwnerId)
        {
            var basket = await GetBasketWithDetailsAsync(businessOwnerId)
                ?? throw new KeyNotFoundException("Basket not found.");

            foreach (var item in basket.Items)
                _unitOfWork.Repository<MaterialBasketItem>().Delete(item);

            await _unitOfWork.CompleteAsync();
        }

        // ── Private helpers ───────────────────────────────────────

        private async Task<MaterialBasket> GetOrCreateBasketAsync(string businessOwnerId)
        {
            var spec = new MaterialBasketWithItemsSpec(businessOwnerId);
            var basket = await _unitOfWork.Repository<MaterialBasket>()
                .GetByIdWithSpecificationsAsync(spec);

            if (basket != null) return basket;

            // First access — create an empty basket for this BO
            var newBasket = new MaterialBasket { BusinessOwnerId = businessOwnerId };
            _unitOfWork.Repository<MaterialBasket>().Add(newBasket);
            await _unitOfWork.CompleteAsync();
            return newBasket;
        }

        private async Task<MaterialBasket?> GetBasketWithDetailsAsync(string businessOwnerId)
        {
            var spec = new MaterialBasketWithItemsSpec(businessOwnerId);
            return await _unitOfWork.Repository<MaterialBasket>()
                .GetByIdWithSpecificationsAsync(spec);
        }
    }
}
