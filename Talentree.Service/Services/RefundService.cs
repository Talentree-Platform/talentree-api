using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.OrderSpecifications;
using Talentree.Core.Specifications.RefundSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;
using Talentree.Service.DTOs.Refund;
using Stripe;

namespace Talentree.Service.Services
{
    public class RefundService : IRefundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RefundService> _logger;

        public RefundService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService, ILogger<RefundService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<RefundRequestDto> SubmitRefundRequestAsync(int orderId, int itemId, string customerId, CreateRefundRequestDto dto)
        {
            var spec = new AdminOrderByIdSpecification(orderId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(spec);

            if (order == null || order.CustomerId != customerId)
                throw new NotFoundException($"Order #{orderId} not found.");

            if (order.Status != CustomerOrderStatus.Delivered)
                throw new BadRequestException("Refunds can only be requested for delivered orders.");

            if (order.PaymentStatus != PaymentStatus.Paid && order.PaymentStatus != PaymentStatus.Unpaid) // Unpaid could be COD
                throw new BadRequestException("Order is not eligible for a refund.");

            var item = order.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new NotFoundException($"Item #{itemId} not found in this order.");

            var existingSpec = new RefundRequestsByItemSpecification(orderId, itemId, customerId);
            var existingRequests = await _unitOfWork.Repository<RefundRequest>().GetAllWithSpecificationsAsync(existingSpec);

            if (existingRequests.Any(r => r.Status == RefundStatus.PendingBoResponse || r.Status == RefundStatus.PendingAdminApproval || r.Status == RefundStatus.Approved))
                throw new BadRequestException("A pending or approved refund request already exists for this item.");

            var boId = item.Product?.BusinessOwner?.UserId;
            if (string.IsNullOrEmpty(boId))
                throw new InvalidOperationException("Could not determine the Business Owner for this item.");

            var request = new RefundRequest
            {
                OrderId = orderId,
                OrderItemId = itemId,
                CustomerId = customerId,
                BusinessOwnerId = boId,
                Reason = dto.Reason,
                RefundAmount = item.LineTotal,
                Status = RefundStatus.PendingBoResponse
            };

            _unitOfWork.Repository<RefundRequest>().Add(request);
            await _unitOfWork.CompleteAsync();

            // Notify BO
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = boId,
                Type = NotificationType.Refund,
                Title = "New Refund Request",
                Message = $"A customer requested a refund for {item.ProductName} from order #{orderId}.",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "RefundRequest",
                RelatedEntityId = request.Id
            });

            return _mapper.Map<RefundRequestDto>(request);
        }

        public async Task<Pagination<RefundRequestDto>> GetBoRefundRequestsAsync(string boId, RefundStatus? status, int pageIndex, int pageSize)
        {
            var countSpec = new BoRefundRequestsCountSpecification(boId, status);
            var totalCount = await _unitOfWork.Repository<RefundRequest>().GetCountWithSpecificationsAsync(countSpec);

            var spec = new BoRefundRequestsSpecification(boId, status, pageIndex, pageSize);
            var requests = await _unitOfWork.Repository<RefundRequest>().GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<System.Collections.Generic.List<RefundRequestDto>>(requests);
            return new Pagination<RefundRequestDto>(pageIndex, pageSize, totalCount, dtos);
        }

        public async Task<RefundRequestDto> RespondToRefundRequestAsync(int requestId, string boId, RespondRefundDto dto)
        {
            var spec = new BoRefundRequestsSpecification(requestId, boId);
            var request = await _unitOfWork.Repository<RefundRequest>().GetByIdWithSpecificationsAsync(spec);

            if (request == null)
                throw new NotFoundException($"Refund Request #{requestId} not found.");

            if (request.Status != RefundStatus.PendingBoResponse)
                throw new BadRequestException("This request has already been responded to.");

            request.BoResponse = dto.Response;
            request.BoNotes = dto.Notes;
            request.Status = RefundStatus.PendingAdminApproval;

            _unitOfWork.Repository<RefundRequest>().Update(request);
            await _unitOfWork.CompleteAsync();

            // Note: We don't notify the admin here, they have a dashboard to check

            return _mapper.Map<RefundRequestDto>(request);
        }

        public async Task<Pagination<RefundRequestDto>> GetPendingRefundsAsync(RefundStatus? status, int pageIndex, int pageSize)
        {
            var countSpec = new AdminRefundRequestsCountSpecification(status);
            var totalCount = await _unitOfWork.Repository<RefundRequest>().GetCountWithSpecificationsAsync(countSpec);

            var spec = new AdminRefundRequestsSpecification(status, pageIndex, pageSize);
            var requests = await _unitOfWork.Repository<RefundRequest>().GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<System.Collections.Generic.List<RefundRequestDto>>(requests);
            return new Pagination<RefundRequestDto>(pageIndex, pageSize, totalCount, dtos);
        }

        public async Task<RefundRequestDto> ApproveRefundAsync(int requestId, string adminId)
        {
            var spec = new AdminRefundRequestsSpecification(RefundStatus.PendingAdminApproval, 1, 1); // Get specific doesn't exist, we'll use base
            var request = await _unitOfWork.Repository<RefundRequest>().GetByIdAsync(requestId);
            
            if (request == null)
                throw new NotFoundException($"Refund Request #{requestId} not found.");

            if (request.Status != RefundStatus.PendingAdminApproval && request.Status != RefundStatus.PendingBoResponse)
                throw new BadRequestException("Request is not pending approval.");

            // Load full order
            var orderSpec = new AdminOrderByIdSpecification(request.OrderId);
            var order = await _unitOfWork.Repository<CustomerOrder>().GetByIdWithSpecificationsAsync(orderSpec);

            if (order == null)
                throw new NotFoundException("Associated order not found.");

            if (order.PaymentMethod == Talentree.Core.Enums.PaymentMethod.CreditCard && !string.IsNullOrEmpty(order.StripePaymentIntentId))
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = order.StripePaymentIntentId,
                    Amount = (long)(request.RefundAmount * 100) // partial refund
                };
                
                var stripeRefundService = new Stripe.RefundService();
                try
                {
                    var stripeRefund = await stripeRefundService.CreateAsync(options);
                    request.StripeRefundId = stripeRefund.Id;
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Stripe Refund failed for Order {OrderId}", order.Id);
                    throw new BadRequestException($"Stripe Refund failed: {ex.StripeError.Message}");
                }
            }

            request.Status = RefundStatus.Approved;
            request.ProcessedById = adminId;
            request.ProcessedAt = DateTime.UtcNow;

            // Update BO Ledger
            var transaction = new Talentree.Core.Entities.Transaction
            {
                BusinessOwnerId = request.BusinessOwnerId,
                Type = TransactionType.Refund,
                Amount = -request.RefundAmount, // Debit
                Description = $"Refund processed for item in Order #{order.Id}",
                ReferenceId = order.Id,
                ReferenceType = "CustomerOrder",
                BalanceAfter = 0 // Normally calculated during saving by Financial/Payment service, let's calculate it
            };

            // Calculate balance: find last transaction
            var transSpec = new Talentree.Core.Specifications.Transactions.TransactionsByBoSpecification(request.BusinessOwnerId, null, 1, 1);
            var lastTrans = (await _unitOfWork.Repository<Talentree.Core.Entities.Transaction>().GetAllWithSpecificationsAsync(transSpec)).FirstOrDefault();
            transaction.BalanceAfter = (lastTrans?.BalanceAfter ?? 0) + transaction.Amount;

            _unitOfWork.Repository<Talentree.Core.Entities.Transaction>().Add(transaction);

            // Notify Customer
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = request.CustomerId,
                Type = NotificationType.Refund,
                Title = "Refund Approved ✅",
                Message = $"Your refund of {request.RefundAmount} EGP for order #{request.OrderId} has been approved.",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "RefundRequest",
                RelatedEntityId = request.Id
            });

            // Notify BO
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = request.BusinessOwnerId,
                Type = NotificationType.Refund,
                Title = "Refund Processed",
                Message = $"A refund of {request.RefundAmount} EGP was processed for your item in order #{request.OrderId}.",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "RefundRequest",
                RelatedEntityId = request.Id
            });

            _unitOfWork.Repository<RefundRequest>().Update(request);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<RefundRequestDto>(request);
        }

        public async Task<RefundRequestDto> RejectRefundAsync(int requestId, string adminId, RejectRefundDto dto)
        {
            var request = await _unitOfWork.Repository<RefundRequest>().GetByIdAsync(requestId);
            
            if (request == null)
                throw new NotFoundException($"Refund Request #{requestId} not found.");

            if (request.Status != RefundStatus.PendingAdminApproval && request.Status != RefundStatus.PendingBoResponse)
                throw new BadRequestException("Request is not pending approval.");

            request.Status = RefundStatus.Rejected;
            request.AdminNotes = dto.Reason;
            request.ProcessedById = adminId;
            request.ProcessedAt = DateTime.UtcNow;

            _unitOfWork.Repository<RefundRequest>().Update(request);
            await _unitOfWork.CompleteAsync();

            // Notify Customer
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = request.CustomerId,
                Type = NotificationType.Refund,
                Title = "Refund Rejected ❌",
                Message = $"Your refund request for order #{request.OrderId} was rejected. Reason: {dto.Reason}",
                Priority = NotificationPriority.High,
                SendEmail = true,
                RelatedEntityType = "RefundRequest",
                RelatedEntityId = request.Id
            });

            return _mapper.Map<RefundRequestDto>(request);
        }
    }
}
