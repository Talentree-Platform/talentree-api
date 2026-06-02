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
using Talentree.Core.Specifications.OrderSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Customer;
using Talentree.Service.DTOs.Notification;
using Talentree.Service.DTOs.Payment;

namespace Talentree.Service.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaymentService _paymentService;
        private readonly INotificationHelperService _notificationHelper;  
        private readonly ILogger<CustomerOrderService> _logger;
        private readonly INotificationService _notificationService;

        public CustomerOrderService(IUnitOfWork unitOfWork, IMapper mapper, IPaymentService paymentService,
            INotificationHelperService notificationHelper, ILogger<CustomerOrderService> logger, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _paymentService = paymentService;
            _notificationHelper = notificationHelper;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<CustomerOrderDetailDto> PlaceOrderAsync(CheckoutDeliveryDto delivery, PaymentMethod method, string customerId)
        {
            // 1. Fetch the cart
            var cartSpec = new CartByCustomerSpecification(customerId);
            var carts = await _unitOfWork.Repository<CustomerCart>().GetAllWithSpecificationsAsync(cartSpec);
            var cart = carts.FirstOrDefault();
            if (cart == null || !cart.Items.Any())
                throw new BadRequestException("Your cart is empty.");

            // 2. Validate stock and build snapshotted items
            var orderItems = new List<CustomerOrderItem>();
            decimal subtotal = 0m;

            foreach (var item in cart.Items)
            {
                var product = item.Product;
                if (product == null)
                    throw new NotFoundException($"Product {item.ProductId} not found.");

                if (product.StockQuantity < item.Quantity)
                    throw new BadRequestException($"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {item.Quantity}");

                // Snapshot price and details
                var orderItem = new CustomerOrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    ProductImageUrl = product.Images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImageUrl,
                    SellerName = product.BusinessOwner?.BusinessName ?? "Unknown Seller",
                    UnitPrice = product.Price,
                    Quantity = item.Quantity
                };

                orderItems.Add(orderItem);
                subtotal += product.Price * item.Quantity;

                // Decrement stock
                product.StockQuantity -= item.Quantity;
                _unitOfWork.Repository<Product>().Update(product);
            }

            // 3. Calculate shipping and total
            decimal shipping = subtotal > 0 && subtotal < 100m ? 10m : 0m;
            decimal total = subtotal + shipping;

            // 4. Determine status based on payment method
            var status = method == PaymentMethod.CashOnDelivery ? CustomerOrderStatus.Processing : CustomerOrderStatus.Pending;
            var paymentStatus = PaymentStatus.Unpaid;

            // 5. Create Order
            var order = new CustomerOrder
            {
                CustomerId = customerId,
                FullName = delivery.FullName,
                PhoneNumber = delivery.PhoneNumber,
                Street = delivery.Street,
                City = delivery.City,
                PostalCode = delivery.PostalCode,
                Country = delivery.Country,
                SubtotalAmount = subtotal,
                ShippingAmount = shipping,
                TotalAmount = total,
                Status = status,
                PaymentStatus = paymentStatus,
                PaymentMethod = method,
                Items = orderItems,
                StatusHistory = new List<OrderStatusHistory>()
            };

            // Add initial history
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = status,
                Notes = method == PaymentMethod.CashOnDelivery 
                    ? "Order placed and is being processed (Cash on Delivery)." 
                    : "Order placed. Pending credit card payment.",
                ChangedBy = customerId,
                ChangedAt = DateTime.UtcNow
            });

            _unitOfWork.Repository<CustomerOrder>().Add(order);

            // 6. Clear Cart
            foreach (var item in cart.Items.ToList())
            {
                _unitOfWork.Repository<CustomerCartItem>().Delete(item);
            }

            // 7. Commit changes
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<CustomerOrderDetailDto>(order);

            // ✅ ADD NOTIFICATION TO CUSTOMER
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = customerId,
                Type = NotificationType.Order,
                Title = "Order Placed Successfully ✅",
                Message = $"Your order #{order.Id} has been placed. Total: {order.TotalAmount} EGP.",
                ActionUrl = $"/orders/{order.Id}",
                ActionText = "View Order",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            });

            // ✅ NOTIFY SELLERS 
            var sellerIds = order.Items
                .Select(i => i.Product?.BusinessOwner?.UserId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            foreach (var sellerId in sellerIds)
            {
                if (sellerId != null)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = sellerId,
                        Type = NotificationType.Order,
                        Title = "New Order Received 📦",
                        Message = $"You have a new order #{order.Id} with {order.Items.Count} item(s).",
                        ActionUrl = $"/seller/orders/{order.Id}",
                        ActionText = "View Order Details",
                        Priority = NotificationPriority.High,
                        SendEmail = true,
                        RelatedEntityType = "Order",
                        RelatedEntityId = order.Id
                    });
                }
            }

            _logger.LogInformation("Order {OrderId} placed by customer {CustomerId}. Total: {Total} EGP. Payment Method: {Method}",
                order.Id, customerId, order.TotalAmount, order.PaymentMethod);

            return result;
        }

        public async Task<OrderPaymentDto> CreatePaymentIntentAsync(int orderId, string customerId)
        {
            var spec = new CustomerOrderByIdSpecification(orderId, customerId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);
            if (order == null)
                throw new NotFoundException($"Order #{orderId} not found.");

            if (order.PaymentMethod != PaymentMethod.CreditCard)
                throw new BadRequestException("This order is not configured for Credit Card payment.");

            if (order.PaymentStatus == PaymentStatus.Paid)
                throw new BadRequestException("This order has already been paid.");

            var intentDto = await _paymentService.CreateCustomerOrderIntentAsync(orderId, customerId);

            return new OrderPaymentDto
            {
                OrderId = orderId,
                PaymentMethod = order.PaymentMethod.ToString(),
                StripeClientSecret = intentDto.ClientSecret
            };
        }

        public async Task<Pagination<CustomerOrderSummaryDto>> GetOrdersAsync(string customerId, OrderFilterDto filter)
        {
            var filterParams = new CustomerOrderFilterParams
            {
                Search = filter.Search,
                Status = filter.Status,
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize
            };

            var countSpec = new CustomerOrdersSpecification(customerId, filterParams, true);
            var totalCount = await _unitOfWork.Repository<CustomerOrder>().GetCountWithSpecificationsAsync(countSpec);

            var spec = new CustomerOrdersSpecification(customerId, filterParams);
            var orders = await _unitOfWork.Repository<CustomerOrder>().GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<CustomerOrderSummaryDto>>(orders);

            return new Pagination<CustomerOrderSummaryDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        public async Task<CustomerOrderDetailDto> GetOrderByIdAsync(int orderId, string customerId)
        {
            var spec = new CustomerOrderByIdSpecification(orderId, customerId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);
            if (order == null)
                throw new NotFoundException($"Order #{orderId} not found.");

            return _mapper.Map<CustomerOrderDetailDto>(order);
        }

        public async Task CancelOrderAsync(int orderId, string customerId)
        {
            var spec = new CustomerOrderByIdSpecification(orderId, customerId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);
            if (order == null)
                throw new NotFoundException($"Order #{orderId} not found.");

            if (order.Status != CustomerOrderStatus.Pending)
                throw new BadRequestException("Only pending orders can be cancelled.");

            order.Status = CustomerOrderStatus.Cancelled;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = CustomerOrderStatus.Cancelled,
                Notes = "Order cancelled by customer.",
                ChangedBy = customerId,
                ChangedAt = DateTime.UtcNow
            });

            // Restore stock levels
            foreach (var item in order.Items)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    _unitOfWork.Repository<Product>().Update(product);
                }
            }

            _unitOfWork.Repository<CustomerOrder>().Update(order);
            await _unitOfWork.CompleteAsync();
            //var result = _mapper.Map<CustomerOrderDetailDto>(order);

            // ✅ ADD NOTIFICATION TO CUSTOMER
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = customerId,
                Type = NotificationType.Order,
                Title = "Order Cancelled ❌",
                Message = $"Your order #{order.Id} has been cancelled. Amount will be refunded shortly.",
                ActionUrl = $"/orders/{order.Id}",
                ActionText = "View Order",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            });

            // ✅ NOTIFY SELLERS
            var sellerIds = order.Items
                .Select(i => i.Product?.BusinessOwner?.UserId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            foreach (var sellerId in sellerIds)
            {
                if (sellerId != null)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = sellerId,
                        Type = NotificationType.Order,
                        Title = "Order Cancelled",
                        Message = $"Order #{order.Id} has been cancelled by the customer.",
                        ActionUrl = $"/seller/orders/{order.Id}",
                        ActionText = "View Order",
                        Priority = NotificationPriority.Normal,
                        SendEmail = true,
                        RelatedEntityType = "Order",
                        RelatedEntityId = order.Id
                    });
                }
            }

            _logger.LogInformation("Order {OrderId} cancelled by customer {CustomerId}", order.Id, customerId);

            
        }

        public async Task<byte[]> GenerateInvoiceAsync(int orderId, string customerId)
        {
            var spec = new CustomerOrderByIdSpecification(orderId, customerId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);
            if (order == null)
                throw new NotFoundException($"Order #{orderId} not found.");

            // Return a simple PDF stub (PDF header + text)
            string pdfText = $"%PDF-1.4\n1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R >>\nendobj\n4 0 obj\n<< /Length 100 >>\nstream\nBT\n/F1 12 Tf\n70 700 Td\n(Talentree E-Commerce Platform - Invoice for Order #{order.Id}) Tj\n70 680 Td\n(Customer: {order.FullName}) Tj\n70 660 Td\n(Total Amount: {order.TotalAmount} EGP) Tj\n70 640 Td\n(Payment Method: {order.PaymentMethod}) Tj\nET\nendstream\nendobj\nxref\n0 5\n0000000000 65535 f\n0000000009 00000 n\n0000000056 00000 n\n0000000111 00000 n\n0000000212 00000 n\ntrailer\n<< /Size 5 /Root 1 0 R >>\nstartxref\n362\n%%EOF";
            return System.Text.Encoding.UTF8.GetBytes(pdfText);
        }
    }
}
