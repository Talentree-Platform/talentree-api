using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.Basket;
using Talentree.Core.Specifications.MaterialOrders;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.MaterialOrder;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Handles the BO material checkout flow and order history.
    /// Checkout is fully transactional: stock deduction, order creation,
    /// and basket clearing succeed or fail together in a single <c>CompleteAsync</c> call.
    /// </summary>
    public class MaterialOrderService : IMaterialOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MaterialOrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<MaterialOrderDto> CheckoutAsync(
            string businessOwnerId, MaterialCheckoutDto dto)
        {
            // 1 — Load basket with all items and their raw material data
            var basketSpec = new MaterialBasketWithItemsSpec(businessOwnerId);
            var basket = await _unitOfWork.Repository<MaterialBasket>()
                .GetByIdWithSpecificationsAsync(basketSpec);

            if (basket is null || !basket.Items.Any())
                throw new InvalidOperationException("Your basket is empty.");

            // 2 — Re-validate stock at checkout time
            //     (another BO may have purchased stock since items were added)
            foreach (var item in basket.Items)
            {
                var material = item.RawMaterial
                    ?? throw new InvalidOperationException(
                        $"Material data could not be loaded for basket item #{item.Id}.");

                if (material.IsDeleted || !material.IsAvailable)
                    throw new InvalidOperationException(
                        $"'{material.Name}' is no longer available. Please remove it from your basket.");

                if (item.Quantity > material.StockQuantity)
                    throw new InvalidOperationException(
                        $"Only {material.StockQuantity} units of '{material.Name}' remain in stock. " +
                        $"Please update your basket quantity.");
            }

            // 3 — Build the order with prices locked at this moment
            var order = new MaterialOrder
            {
                BusinessOwnerId = businessOwnerId,
                DeliveryAddress = dto.DeliveryAddress,
                DeliveryCity = dto.DeliveryCity,
                DeliveryCountry = dto.DeliveryCountry,
                ContactPhone = dto.ContactPhone,
                PaymentStatus = PaymentStatus.Unpaid,    
                Status = MaterialOrderStatus.Pending,
                Items = basket.Items.Select(i => new MaterialOrderItem
                {
                    RawMaterialId = i.RawMaterialId,
                    Quantity = i.Quantity,
                    UnitPriceAtPurchase = i.RawMaterial!.Price
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(i => i.LineTotal);

            // 4 — Deduct stock from each raw material
            foreach (var item in basket.Items)
                item.RawMaterial!.StockQuantity -= item.Quantity;

            // 5 — Clear basket items
            foreach (var item in basket.Items.ToList())
                _unitOfWork.Repository<MaterialBasketItem>().Delete(item);

            // 6 — Persist order, stock changes, and basket clear in one transaction
            _unitOfWork.Repository<MaterialOrder>().Add(order);
            await _unitOfWork.CompleteAsync();

            // 7 — Reload the order with all includes for the response DTO
            var detailSpec = new MaterialOrderByIdAndBoSpecification(order.Id, businessOwnerId);
            var created = await _unitOfWork.Repository<MaterialOrder>()
                .GetByIdWithSpecificationsAsync(detailSpec);

            return _mapper.Map<MaterialOrderDto>(created);
        }

        /// <inheritdoc/>
        public async Task<Pagination<MaterialOrderSummaryDto>> GetOrderHistoryAsync(
            string businessOwnerId, int pageIndex, int pageSize)
        {
            var listSpec = new MaterialOrdersByBoSpecification(businessOwnerId, pageIndex, pageSize);
            var countSpec = new MaterialOrdersCountByBoSpecification(businessOwnerId);

            var orders = await _unitOfWork.Repository<MaterialOrder>()
                .GetAllWithSpecificationsAsync(listSpec);
            var total = await _unitOfWork.Repository<MaterialOrder>()
                .GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<MaterialOrderSummaryDto>>(orders);
            return new Pagination<MaterialOrderSummaryDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<MaterialOrderDto> GetOrderByIdAsync(string businessOwnerId, int orderId)
        {
            var spec = new MaterialOrderByIdAndBoSpecification(orderId, businessOwnerId);
            var order = await _unitOfWork.Repository<MaterialOrder>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Order #{orderId} not found.");

            return _mapper.Map<MaterialOrderDto>(order);
        }
    }
}