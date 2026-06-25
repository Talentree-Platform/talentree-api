// Talentree.Service/Services/AdminProductService.cs

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.ProductSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin.Product;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Notification;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Implements admin product management for FR-AD-09, FR-AD-10, and FR-AD-11.
    /// </summary>
    public class AdminProductService : IAdminProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AdminProductService> _logger;
        private readonly UserManager<AppUser> _userManager;

        public AdminProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<AdminProductService> logger,
            UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _logger = logger;
            _userManager = userManager;
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-09: Approval Queue — Detail + Bulk Actions
        // ═══════════════════════════════════════════════════════════

        public async Task<AdminProductDetailDto> GetProductDetailAsync(int productId)
        {
            var spec = new ProductByIdSpecification(productId, ignoreQueryFilters: true);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException($"Product with ID {productId} not found.");

            return _mapper.Map<AdminProductDetailDto>(product);
        }

        public async Task BulkApproveAsync(BulkProductActionDto dto, string adminId)
        {
            if (dto.ProductIds == null || dto.ProductIds.Count == 0)
                throw new BadRequestException("At least one ProductId is required.");

            var approved = new List<int>();

            foreach (var productId in dto.ProductIds)
            {
                var spec = new ProductByIdSpecification(productId, ignoreQueryFilters: true);
                var product = await _unitOfWork.Repository<Product>()
                    .GetByIdWithSpecificationsAsync(spec);

                if (product == null)
                {
                    _logger.LogWarning("Bulk approve: product {Id} not found — skipped.", productId);
                    continue;
                }

                if (product.Status != ProductStatus.PendingApproval &&
                    product.Status != ProductStatus.RequestedChanges)
                {
                    _logger.LogWarning("Bulk approve: product {Id} is in status {Status} — skipped.", productId, product.Status);
                    continue;
                }

                product.Status = ProductStatus.Approved;
                product.ApprovedAt = DateTime.UtcNow;
                product.ApprovedBy = adminId;
                product.RejectionReason = null;
                _unitOfWork.Repository<Product>().Update(product);

                approved.Add(productId);

                // Notify seller per product
                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = product.BusinessOwner.UserId,
                    Type = NotificationType.Product,
                    Title = "Product Approved! ✅",
                    Message = $"Your product '{product.Name}' has been approved and is now live on Talentree.",
                    ActionUrl = $"/products/{product.Id}",
                    ActionText = "View Product",
                    RelatedEntityType = "Product",
                    RelatedEntityId = product.Id,
                    Priority = NotificationPriority.High,
                    SendEmail = true
                });
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Bulk approve: {Count} products approved by admin {AdminId}.", approved.Count, adminId);
        }

        public async Task BulkRejectAsync(BulkProductActionDto dto, string adminId)
        {
            if (dto.ProductIds == null || dto.ProductIds.Count == 0)
                throw new BadRequestException("At least one ProductId is required.");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new BadRequestException("A rejection reason is required for bulk reject.");

            foreach (var productId in dto.ProductIds)
            {
                var spec = new ProductByIdSpecification(productId, ignoreQueryFilters: true);
                var product = await _unitOfWork.Repository<Product>()
                    .GetByIdWithSpecificationsAsync(spec);

                if (product == null)
                {
                    _logger.LogWarning("Bulk reject: product {Id} not found — skipped.", productId);
                    continue;
                }

                if (product.Status != ProductStatus.PendingApproval &&
                    product.Status != ProductStatus.RequestedChanges)
                {
                    _logger.LogWarning("Bulk reject: product {Id} has status {Status} — skipped.", productId, product.Status);
                    continue;
                }

                product.Status = ProductStatus.Rejected;
                product.RejectionReason = dto.Reason;
                product.ApprovedAt = null;
                product.ApprovedBy = null;
                _unitOfWork.Repository<Product>().Update(product);

                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = product.BusinessOwner.UserId,
                    Type = NotificationType.Product,
                    Title = "Product Not Approved",
                    Message = $"Your product '{product.Name}' was not approved. Reason: {dto.Reason}",
                    ActionUrl = $"/products/{product.Id}/edit",
                    ActionText = "Edit Product",
                    RelatedEntityType = "Product",
                    RelatedEntityId = product.Id,
                    Priority = NotificationPriority.High,
                    SendEmail = true
                });
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Bulk reject: {Count} products rejected by admin {AdminId}.", dto.ProductIds.Count, adminId);
        }

        public async Task RequestChangesAsync(RejectProductDto dto, string adminId)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new BadRequestException("A reason is required when requesting changes.");

            var spec = new ProductByIdSpecification(dto.ProductId, ignoreQueryFilters: true);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException($"Product {dto.ProductId} not found.");

            if (product.Status != ProductStatus.PendingApproval)
                throw new BadRequestException("Product is not in PendingApproval status.");

            product.Status = ProductStatus.RequestedChanges;
            product.RejectionReason = dto.Reason;
            product.ApprovedAt = null;
            product.ApprovedBy = null;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = product.BusinessOwner.UserId,
                Type = NotificationType.Product,
                Title = "Changes Requested for Your Product",
                Message = $"Your product '{product.Name}' needs updates before approval. Reason: {dto.Reason}",
                ActionUrl = $"/products/{product.Id}/edit",
                ActionText = "Edit Product",
                RelatedEntityType = "Product",
                RelatedEntityId = product.Id,
                Priority = NotificationPriority.High,
                SendEmail = true
            });

            _logger.LogInformation("Changes requested for product {ProductId} by admin {AdminId}.", dto.ProductId, adminId);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-10: Product Moderation
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<AdminProductDto>> GetAllProductsAsync(AdminProductFilterDto filter)
        {
            if (filter.PageIndex < 1) filter.PageIndex = 1;
            if (filter.PageSize < 1) filter.PageSize = 20;
            if (filter.PageSize > 100) filter.PageSize = 100;

            var countSpec = new AdminProductsSpecification(
                status: filter.Status,
                categoryId: filter.CategoryId,
                businessOwnerProfileId: filter.BusinessOwnerProfileId,
                minPrice: filter.MinPrice,
                maxPrice: filter.MaxPrice,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                search: filter.Search,
                sortBy: filter.SortBy,
                sortDesc: filter.SortDesc);

            var totalCount = await _unitOfWork.Repository<Product>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new AdminProductsSpecification(
                pageIndex: filter.PageIndex,
                pageSize: filter.PageSize,
                status: filter.Status,
                categoryId: filter.CategoryId,
                businessOwnerProfileId: filter.BusinessOwnerProfileId,
                minPrice: filter.MinPrice,
                maxPrice: filter.MaxPrice,
                fromDate: filter.FromDate,
                toDate: filter.ToDate,
                search: filter.Search,
                sortBy: filter.SortBy,
                sortDesc: filter.SortDesc);

            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<AdminProductDto>>(products);

            return new Pagination<AdminProductDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        public async Task HideProductAsync(int productId, string adminId)
        {
            var product = await GetProductOrThrowAsync(productId, ignoreQueryFilters: false);
            if (!product.IsVisible)
                throw new BadRequestException("Product is already hidden.");

            product.IsVisible = false;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = adminId;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = product.BusinessOwner.UserId,
                Type = NotificationType.Product,
                Title = "Product Hidden",
                Message = $"Your product '{product.Name}' has been temporarily hidden from the marketplace by admin review.",
                ActionUrl = $"/products/{product.Id}",
                ActionText = "View Product",
                RelatedEntityType = "Product",
                RelatedEntityId = product.Id,
                Priority = NotificationPriority.High,
                SendEmail = false
            });

            _logger.LogInformation("Product {ProductId} hidden by admin {AdminId}.", productId, adminId);
        }

        public async Task RestoreProductAsync(int productId, string adminId)
        {
            var product = await GetProductOrThrowAsync(productId, ignoreQueryFilters: false);
            if (product.IsVisible)
                throw new BadRequestException("Product is already visible.");

            product.IsVisible = true;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = adminId;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = product.BusinessOwner.UserId,
                Type = NotificationType.Product,
                Title = "Product Restored ✅",
                Message = $"Your product '{product.Name}' is now visible on the marketplace again.",
                ActionUrl = $"/products/{product.Id}",
                ActionText = "View Product",
                RelatedEntityType = "Product",
                RelatedEntityId = product.Id,
                Priority = NotificationPriority.Normal,
                SendEmail = false
            });

            _logger.LogInformation("Product {ProductId} restored by admin {AdminId}.", productId, adminId);
        }

        public async Task FeatureProductAsync(int productId, string adminId)
        {
            var product = await GetProductOrThrowAsync(productId, ignoreQueryFilters: false);
            if (product.IsFeatured)
                throw new BadRequestException("Product is already featured.");

            product.IsFeatured = true;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = adminId;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product {ProductId} featured by admin {AdminId}.", productId, adminId);
        }

        public async Task UnfeatureProductAsync(int productId, string adminId)
        {
            var product = await GetProductOrThrowAsync(productId, ignoreQueryFilters: false);
            if (!product.IsFeatured)
                throw new BadRequestException("Product is not currently featured.");

            product.IsFeatured = false;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = adminId;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product {ProductId} unfeatured by admin {AdminId}.", productId, adminId);
        }

        public async Task ChangeProductCategoryAsync(ChangeProductCategoryDto dto, string adminId)
        {
            var product = await GetProductOrThrowAsync(dto.ProductId, ignoreQueryFilters: false);

            // Validate the new category exists
            var category = await _unitOfWork.Repository<Category>()
                .GetByIdAsync(dto.NewCategoryId);
            if (category == null)
                throw new NotFoundException($"Category {dto.NewCategoryId} not found.");

            if (product.CategoryId == dto.NewCategoryId)
                throw new BadRequestException("Product is already in the specified category.");

            product.CategoryId = dto.NewCategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = adminId;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product {ProductId} category changed to {CategoryId} by admin {AdminId}.",
                dto.ProductId, dto.NewCategoryId, adminId);
        }

        public async Task<AdminProductAnalyticsDto> GetProductAnalyticsAsync(int productId)
        {
            var spec = new ProductByIdSpecification(productId, ignoreQueryFilters: true);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException($"Product {productId} not found.");

            return _mapper.Map<AdminProductAnalyticsDto>(product);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-AD-11: Low Stock Alerts
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<LowStockProductDto>> GetLowStockProductsAsync(LowStockFilterDto filter)
        {
            if (filter.PageIndex < 1) filter.PageIndex = 1;
            if (filter.PageSize < 1) filter.PageSize = 20;
            if (filter.PageSize > 100) filter.PageSize = 100;

            var countSpec = new LowStockProductsSpecification(
                categoryId: filter.CategoryId,
                businessOwnerProfileId: filter.BusinessOwnerProfileId);
            var totalCount = await _unitOfWork.Repository<Product>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new LowStockProductsSpecification(
                pageIndex: filter.PageIndex,
                pageSize: filter.PageSize,
                categoryId: filter.CategoryId,
                businessOwnerProfileId: filter.BusinessOwnerProfileId);
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<LowStockProductDto>>(products);
            return new Pagination<LowStockProductDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        public async Task NotifySellerLowStockAsync(int productId, string adminId)
        {
            var spec = new ProductByIdSpecification(productId, ignoreQueryFilters: false);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException($"Product {productId} not found.");

            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = product.BusinessOwner.UserId,
                Type = NotificationType.Product,
                Title = "Low Stock Alert ⚠️",
                Message = $"Your product '{product.Name}' is running low on stock ({product.StockQuantity} units remaining). Please restock soon.",
                ActionUrl = $"/products/{product.Id}/edit",
                ActionText = "Update Stock",
                RelatedEntityType = "Product",
                RelatedEntityId = product.Id,
                Priority = NotificationPriority.High,
                SendEmail = true
            });

            _logger.LogInformation("Low-stock notification sent for product {ProductId} by admin {AdminId}.", productId, adminId);
        }

        public async Task BulkNotifySellersLowStockAsync(string adminId)
        {
            // Get all low-stock products (no pagination for bulk operation)
            var spec = new LowStockProductsSpecification();
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(spec);

            // Group by seller to avoid duplicate notifications per seller
            var grouped = products.GroupBy(p => p.BusinessOwner.UserId);
            var notified = 0;

            foreach (var group in grouped)
            {
                var sellerId = group.Key;
                var productNames = string.Join(", ", group.Select(p => $"'{p.Name}' ({p.StockQuantity} left)"));

                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                {
                    UserId = sellerId,
                    Type = NotificationType.Product,
                    Title = "Low Stock Alert ⚠️",
                    Message = $"The following products are running low on stock: {productNames}. Please restock to avoid losing sales.",
                    ActionUrl = "/dashboard/products",
                    ActionText = "Manage Products",
                    RelatedEntityType = "Product",
                    Priority = NotificationPriority.High,
                    SendEmail = true
                });
                notified++;
            }

            _logger.LogInformation("Bulk low-stock notifications sent to {Count} seller(s) by admin {AdminId}.", notified, adminId);
        }

        // ═══════════════════════════════════════════════════════════
        // Private Helpers
        // ═══════════════════════════════════════════════════════════

        private async Task<Product> GetProductOrThrowAsync(int productId, bool ignoreQueryFilters)
        {
            var spec = new ProductByIdSpecification(productId, ignoreQueryFilters);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException($"Product {productId} not found.");

            return product;
        }
    }
}
