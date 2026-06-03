using AutoMapper;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Repository.Contract;
using Talentree.Core.Specifications.Supplier;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin.Supplier;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Specifications.MaterialOrders;
using Talentree.Core.Specifications.UserManagementSpecifications;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Admin-only operations for managing suppliers.
    /// Suppliers are the source of raw materials on the platform.
    /// </summary>
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;
        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper,
                        ILogger<SupplierService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Pagination<SupplierDto>> GetSuppliersAsync(
            string? search, bool? isActive, int pageIndex, int pageSize)
        {
            var spec = new SupplierSpec(search, isActive, pageIndex, pageSize);
            var countSpec = new SupplierCountSpec(search, isActive);

            var suppliers = await _unitOfWork.Repository<Supplier>().GetAllWithSpecificationsAsync(spec);
            var total = await _unitOfWork.Repository<Supplier>().GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<SupplierDto>>(suppliers);
            return new Pagination<SupplierDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<SupplierDto> GetSupplierByIdAsync(int id)
        {
            var spec = new SupplierByIdWithMaterialsSpec(id);
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Supplier #{id} not found.");

            return _mapper.Map<SupplierDto>(supplier);
        }

        /// <inheritdoc/>
        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
        {
            var allSuppliers = await _unitOfWork.Repository<Supplier>()
                .GetAllWithSpecificationsAsync(new SupplierByEmailSpec(dto.Email));

            if (allSuppliers.Any())
                throw new InvalidOperationException($"A supplier with email '{dto.Email}' already exists.");

            var supplier = _mapper.Map<Supplier>(dto);
            supplier.IsActive = true;

            _unitOfWork.Repository<Supplier>().Add(supplier);
            await _unitOfWork.CompleteAsync();
            // ✅ ADD LOGGING
            _logger.LogInformation("Supplier {SupplierName} created with ID {SupplierId}",
                supplier.Name, supplier.Id);

            return _mapper.Map<SupplierDto>(supplier);
        }

        /// <inheritdoc/>
        public async Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Supplier #{id} not found.");

            if (dto.Name != null) supplier.Name = dto.Name;
            if (dto.Description != null) supplier.Description = dto.Description;
            if (dto.Email != null) supplier.Email = dto.Email;
            if (dto.Phone != null) supplier.Phone = dto.Phone;
            if (dto.Address != null) supplier.Address = dto.Address;
            if (dto.City != null) supplier.City = dto.City;
            if (dto.Country != null) supplier.Country = dto.Country;
            if (dto.ContactPerson != null) supplier.ContactPerson = dto.ContactPerson;
            if (dto.TaxId != null) supplier.TaxId = dto.TaxId;
            if (dto.IsActive.HasValue) supplier.IsActive = dto.IsActive.Value;

            _unitOfWork.Repository<Supplier>().Update(supplier);
            await _unitOfWork.CompleteAsync();


            return _mapper.Map<SupplierDto>(supplier);
        }

        /// <inheritdoc/>
        public async Task DeleteSupplierAsync(int id)
        {
            var spec = new SupplierByIdWithMaterialsSpec(id);
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Supplier #{id} not found.");

            var hasActiveMaterials = supplier.RawMaterials.Any(m => m.IsAvailable && !m.IsDeleted);
            if (hasActiveMaterials)
                throw new InvalidOperationException(
                    "Cannot delete supplier with active materials. Deactivate all their materials first.");

            // Soft delete — IsDeleted and DeletedAt are set by AuditInterceptor
            supplier.IsDeleted = true;
            supplier.IsActive = false;

            _unitOfWork.Repository<Supplier>().Update(supplier);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Pagination<SupplierPerformanceDto>> GetSupplierPerformanceAsync(int pageIndex, int pageSize, string? sortBy)
        {
            var spec = new SupplierPerformanceSpecification(pageIndex, pageSize, sortBy);
            var countSpec = new SupplierPerformanceSpecification();

            var suppliers = await _unitOfWork.Repository<Supplier>().GetAllWithSpecificationsAsync(spec);
            var total = await _unitOfWork.Repository<Supplier>().GetCountWithSpecificationsAsync(countSpec);

            var dtos = new List<SupplierPerformanceDto>();

            foreach (var supplier in suppliers)
            {
                // Only consider delivered items for performance metrics
                var deliveredItems = supplier.RawMaterials
                    .SelectMany(rm => rm.MaterialOrderItems)
                    .Where(moi => moi.Order.Status == MaterialOrderStatus.Delivered)
                    .ToList();

                var totalOrders = deliveredItems.Select(moi => moi.MaterialOrderId).Distinct().Count();
                var totalRevenue = deliveredItems.Sum(moi => moi.LineTotal);
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // Fulfillment time calculation
                var distinctDeliveredOrders = deliveredItems.Select(moi => moi.Order).Distinct().ToList();
                double? avgFulfillment = null;
                var fulfilledWithTime = distinctDeliveredOrders.Where(o => o.DeliveredAt.HasValue).ToList();
                if (fulfilledWithTime.Any())
                {
                    avgFulfillment = fulfilledWithTime.Average(o => (o.DeliveredAt!.Value - o.CreatedAt).TotalHours);
                }

                // Reviews
                var totalReviews = supplier.SupplierReviews.Count;
                var avgRating = totalReviews > 0 ? supplier.SupplierReviews.Average(r => r.Rating) : 0;

                // Issues
                var issueRate = 0.0;
                if (totalOrders > 0)
                {
                    // For simplicity, Issue Rate = (Total Complaints / Total Orders) * 100
                    var totalTickets = supplier.SupportTickets.Count;
                    issueRate = ((double)totalTickets / totalOrders) * 100;
                }

                dtos.Add(new SupplierPerformanceDto
                {
                    SupplierId = supplier.Id,
                    SupplierName = supplier.Name,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue,
                    AverageFulfillmentTimeHours = avgFulfillment,
                    TotalReviews = totalReviews,
                    AverageRating = avgRating,
                    IssueRatePercentage = issueRate
                });
            }

            return new Pagination<SupplierPerformanceDto>(pageIndex, pageSize, total, dtos);
        }

        public async Task<Talentree.Service.DTOs.Supplier.SupplierReviewDto> AddSupplierReviewAsync(int supplierId, string boId, Talentree.Service.DTOs.Supplier.CreateSupplierReviewDto dto)
        {
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdAsync(supplierId)
                ?? throw new KeyNotFoundException("Supplier not found");

            var order = await _unitOfWork.Repository<MaterialOrder>().GetByIdAsync(dto.MaterialOrderId)
                ?? throw new KeyNotFoundException("Order not found");

            if (order.BusinessOwnerId != boId)
                throw new InvalidOperationException("You can only review suppliers for your own orders.");

            if (order.Status != MaterialOrderStatus.Delivered)
                throw new InvalidOperationException("You can only review suppliers for delivered orders.");

            // Check if order actually contains items from this supplier
            // We need to load items and their raw materials to be sure
            var spec = new MaterialOrderByIdSpecification(dto.MaterialOrderId);
            var fullOrder = await _unitOfWork.Repository<MaterialOrder>().GetByIdWithSpecificationsAsync(spec);

            if (fullOrder != null && !fullOrder.Items.Any(i => i.RawMaterial.SupplierId == supplierId))
            {
                throw new InvalidOperationException("This order does not contain any items from this supplier.");
            }

            var review = new SupplierReview
            {
                SupplierId = supplierId,
                BusinessOwnerUserId = boId,
                MaterialOrderId = dto.MaterialOrderId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            _unitOfWork.Repository<SupplierReview>().Add(review);
            await _unitOfWork.CompleteAsync();

            var boSpec = new BusinessOwnerByIdSpecification(boId);
            var boUser = await _unitOfWork.Repository<AppUser>().GetByIdWithSpecificationsAsync(boSpec);

            return new Talentree.Service.DTOs.Supplier.SupplierReviewDto
            {
                Id = review.Id,
                SupplierId = review.SupplierId,
                BusinessOwnerName = boUser?.BusinessOwnerProfile?.BusinessName ?? boUser?.UserName ?? "Business Owner",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<Pagination<Talentree.Service.DTOs.Supplier.SupplierReviewDto>> GetSupplierReviewsAsync(int supplierId, int pageIndex, int pageSize)
        {
            var spec = new SupplierReviewsSpecification(supplierId, pageIndex, pageSize);
            var countSpec = new SupplierReviewsSpecification(supplierId);

            var reviews = await _unitOfWork.Repository<SupplierReview>().GetAllWithSpecificationsAsync(spec);
            var total = await _unitOfWork.Repository<SupplierReview>().GetCountWithSpecificationsAsync(countSpec);

            var dtos = reviews.Select(r => new Talentree.Service.DTOs.Supplier.SupplierReviewDto
            {
                Id = r.Id,
                SupplierId = r.SupplierId,
                BusinessOwnerName = r.BusinessOwner?.UserName ?? "Business Owner",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new Pagination<Talentree.Service.DTOs.Supplier.SupplierReviewDto>(pageIndex, pageSize, total, dtos);
        }
    }
}
