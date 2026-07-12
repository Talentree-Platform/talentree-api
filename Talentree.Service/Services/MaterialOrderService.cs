using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.Basket;
using Talentree.Core.Specifications.MaterialOrders;
using Talentree.Service.Messaging;
using Talentree.Service.Messaging.Contracts;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.MaterialOrder;
using Talentree.Service.DTOs.Notification;

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
        private readonly INotificationHelperService _notificationHelper;
        private readonly ILogger<MaterialOrderService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IUserInteractionService _userInteractionService;
        private readonly IEventPublisher _eventPublisher;

        public MaterialOrderService(IUnitOfWork unitOfWork, IMapper mapper,
            INotificationHelperService notificationHelper, ILogger<MaterialOrderService> logger,
            INotificationService notificationService,
            IUserInteractionService userInteractionService,
            IEventPublisher eventPublisher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationHelper = notificationHelper;
            _logger = logger;
            _notificationService = notificationService;
            _userInteractionService = userInteractionService;
            _eventPublisher = eventPublisher;
        }

        /// <inheritdoc/>
        public async Task<MaterialOrderDto> CheckoutAsync(
      string businessOwnerId, MaterialCheckoutDto dto)
        {
            // 1 — Load basket
            var basketSpec = new MaterialBasketWithItemsSpec(businessOwnerId);
            var basket = await _unitOfWork.Repository<MaterialBasket>()
                .GetByIdWithSpecificationsAsync(basketSpec);

            if (basket is null || !basket.Items.Any())
                throw new InvalidOperationException("Your basket is empty.");

            // 2 — Re-validate stock
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

            // ✅ SAVE material data BEFORE CompleteAsync()
            var purchaseData = new List<dynamic>();
            var previousOrdersMap = new Dictionary<int, bool>();  // materialId -> isReorder

            foreach (var item in basket.Items)
            {
                var material = item.RawMaterial!;

                // ✅ Check reorder status BEFORE CompleteAsync
                var previousOrdersSpec = new PreviousMaterialOrdersByOwnerAndMaterialSpecification(
                    businessOwnerId, material.Id);
                var previousOrderCount = await _unitOfWork.Repository<MaterialOrder>()
                    .GetCountWithSpecificationsAsync(previousOrdersSpec);

                var isReorder = previousOrderCount > 0;
                previousOrdersMap[material.Id] = isReorder;

                // ✅ Save all data needed for logging
                purchaseData.Add(new
                {
                    MaterialId = material.Id,
                    MaterialName = material.Name,
                    CategoryName = material.Category ?? "Uncategorized",
                    UnitPrice = material.Price,
                    Quantity = item.Quantity,
                    IsReorder = isReorder
                });
            }

            // 3 — Build the order
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

            // 4 — Deduct stock
            foreach (var item in basket.Items)
                item.RawMaterial!.StockQuantity -= item.Quantity;

            // 5 — Clear basket items
            foreach (var item in basket.Items.ToList())
                _unitOfWork.Repository<MaterialBasketItem>().Delete(item);

            // 6 — Persist order, stock changes, and basket clear
            _unitOfWork.Repository<MaterialOrder>().Add(order);
            await _unitOfWork.CompleteAsync();  // ✅ DbContext disposed here

            // 7 — Reload the order
            var detailSpec = new MaterialOrderByIdAndBoSpecification(order.Id, businessOwnerId);
            var created = await _unitOfWork.Repository<MaterialOrder>()
                .GetByIdWithSpecificationsAsync(detailSpec);

            var result = _mapper.Map<MaterialOrderDto>(created);

            // ✅ ADD NOTIFICATION TO BUSINESS OWNER
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = businessOwnerId,
                Type = NotificationType.MaterialOrder,
                Title = "Material Order Placed ✅",
                Message = $"Your material order #{order.Id} has been placed. Total: {order.TotalAmount} EGP.",
                ActionUrl = $"/material-orders/{order.Id}",
                ActionText = "View Order",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "MaterialOrder",
                RelatedEntityId = order.Id
            });

            // ℹ️ Suppliers are admin-managed entities without user accounts,
            //    so they cannot receive in-app notifications (Notification.UserId
            //    has a FK to AspNetUsers.Id). Log affected suppliers instead.
            var supplierIds = order.Items
                .Select(i => i.RawMaterial?.SupplierId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (supplierIds.Any())
            {
                _logger.LogInformation(
                    "Material order {OrderId} involves supplier(s): {SupplierIds}. " +
                    "Supplier notification skipped (no user accounts).",
                    order.Id, string.Join(", ", supplierIds));
            }

            _logger.LogInformation(
                "Material order {OrderId} placed by business owner {BoId}. Total: {Total} EGP. Items: {ItemCount}",
                order.Id, businessOwnerId, order.TotalAmount, order.Items.Count);

            // ✅ Log purchase/reorder interactions (using centralized background queue)
            if (!string.IsNullOrEmpty(businessOwnerId))
            {
                foreach (var itemData in purchaseData)
                {
                    await _eventPublisher.PublishAsync("interaction.log", new InteractionLogMessage
                    {
                        UserId = businessOwnerId,
                        UserType = UserInteractionType.Owner,
                        ItemId = itemData.MaterialId,
                        ItemType = UserInteractionItemType.RawMaterial,
                        ActionType = itemData.IsReorder
                            ? UserInteractionActionType.Reorder
                            : UserInteractionActionType.Purchase,
                        Category = itemData.CategoryName,
                        Quantity = itemData.Quantity,
                        Price = itemData.UnitPrice
                    });
                }
            }

            return result;
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