using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.OrderSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin.Orders;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Services
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminOrderService> _logger;

        public AdminOrderService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, ILogger<AdminOrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Pagination<AdminOrderSummaryDto>> GetOrdersAsync(AdminOrderFilterDto filter)
        {
            var countSpec = new AdminOrderCountSpecification(filter.Search, filter.Status, filter.PaymentStatus, filter.DateFrom, filter.DateTo);
            var totalCount = await _unitOfWork.Repository<CustomerOrder>().GetCountWithSpecificationsAsync(countSpec);

            var spec = new AdminOrdersSpecification(filter.Search, filter.Status, filter.PaymentStatus, filter.DateFrom, filter.DateTo, filter.SortBy, filter.SortDesc, filter.PageIndex, filter.PageSize);
            var orders = await _unitOfWork.Repository<CustomerOrder>().GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<AdminOrderSummaryDto>>(orders);

            return new Pagination<AdminOrderSummaryDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        public async Task<AdminOrderDetailDto> GetOrderByIdAsync(int orderId)
        {
            var spec = new AdminOrderByIdSpecification(orderId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);
            
            if (order == null)
                throw new NotFoundException($"Order #{orderId} not found.");

            return _mapper.Map<AdminOrderDetailDto>(order);
        }

        public async Task<AdminOrderStatsDto> GetOrderStatsAsync()
        {
            var orders = await _unitOfWork.Repository<CustomerOrder>().GetAllAsync();
            
            var stats = new AdminOrderStatsDto
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == CustomerOrderStatus.Pending),
                ProcessingOrders = orders.Count(o => o.Status == CustomerOrderStatus.Processing),
                ShippedOrders = orders.Count(o => o.Status == CustomerOrderStatus.Shipped),
                DeliveredOrders = orders.Count(o => o.Status == CustomerOrderStatus.Delivered),
                CancelledOrders = orders.Count(o => o.Status == CustomerOrderStatus.Cancelled),
                RefundedOrders = orders.Count(o => o.Status == CustomerOrderStatus.Refunded),
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Count > 0 ? orders.Average(o => o.TotalAmount) : 0
            };

            return stats;
        }

        public async Task<AdminOrderDetailDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto, string adminId)
        {
            var spec = new AdminOrderByIdSpecification(orderId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);
            
            if (order == null)
                throw new NotFoundException($"Order #{orderId} not found.");

            if (order.Status == dto.NewStatus)
                throw new BadRequestException($"Order is already in {dto.NewStatus} status.");

            // Business rule: Can't move backwards from Delivered
            if (order.Status == CustomerOrderStatus.Delivered && dto.NewStatus != CustomerOrderStatus.Refunded)
                throw new BadRequestException("Cannot change status of a Delivered order unless it is being Refunded.");

            order.Status = dto.NewStatus;
            
            if (!string.IsNullOrEmpty(dto.TrackingNumber))
                order.TrackingNumber = dto.TrackingNumber;
                
            if (dto.EstimatedDeliveryDate.HasValue)
                order.EstimatedDeliveryDate = dto.EstimatedDeliveryDate;

            if (dto.NewStatus == CustomerOrderStatus.Delivered && !order.ActualDeliveryDate.HasValue)
                order.ActualDeliveryDate = DateTime.UtcNow;

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = dto.NewStatus,
                Notes = dto.Reason,
                ChangedBy = adminId,
                ChangedAt = DateTime.UtcNow
            });

            _unitOfWork.Repository<CustomerOrder>().Update(order);
            await _unitOfWork.CompleteAsync();

            // Notify Customer
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = order.CustomerId,
                Type = NotificationType.Order,
                Title = "Order Status Updated",
                Message = $"Your order #{order.Id} is now {order.Status}.",
                ActionUrl = $"/orders/{order.Id}",
                ActionText = "View Order",
                Priority = NotificationPriority.Normal,
                SendEmail = true,
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            });

            _logger.LogInformation("Admin {AdminId} updated order {OrderId} status to {NewStatus}", adminId, orderId, dto.NewStatus);

            return _mapper.Map<AdminOrderDetailDto>(order);
        }

        public async Task<AdminOrderDetailDto> AddAdminNoteAsync(int orderId, string note, string adminId)
        {
            var spec = new AdminOrderByIdSpecification(orderId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);
            
            if (order == null)
                throw new NotFoundException($"Order #{orderId} not found.");

            order.AdminNotes = string.IsNullOrEmpty(order.AdminNotes) ? note : $"{order.AdminNotes}\n[{DateTime.UtcNow:yyyy-MM-dd}] {note}";
            
            _unitOfWork.Repository<CustomerOrder>().Update(order);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<AdminOrderDetailDto>(order);
        }

        public async Task<byte[]> ExportOrdersToCsvAsync(AdminOrderFilterDto filter)
        {
            var spec = new AdminOrdersSpecification(filter.Search, filter.Status, filter.PaymentStatus, filter.DateFrom, filter.DateTo, filter.SortBy, filter.SortDesc, 1, 1000000); // Hacky way to get all for export
            var orders = await _unitOfWork.Repository<CustomerOrder>().GetAllWithSpecificationsAsync(spec);

            var sb = new StringBuilder();
            sb.AppendLine("Id,CreatedAt,CustomerName,CustomerEmail,TotalAmount,Status,PaymentStatus");

            foreach (var order in orders)
            {
                sb.AppendLine($"{order.Id},{order.CreatedAt:yyyy-MM-dd HH:mm},{order.Customer.DisplayName},{order.Customer.Email},{order.TotalAmount},{order.Status},{order.PaymentStatus}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
