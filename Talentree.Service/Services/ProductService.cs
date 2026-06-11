using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Core.Specifications.ProductSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Products;
using Talentree.Service.DTOs.Customer;
using Microsoft.Extensions.Logging;

namespace Talentree.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly INotificationService _notificationService;
        private readonly IAIService _aiService;
        private readonly IUserInteractionService _userInteractionService;
        //_logger
        private readonly ILogger<ProductService> _logger;
        public ProductService(
            ILogger<ProductService> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IImageService imageService,
            INotificationService notificationService,
            IAIService aiService,
            IUserInteractionService userInteractionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
            _notificationService = notificationService;
            _aiService = aiService;
            _userInteractionService = userInteractionService;
            _logger = logger;
        }
        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Get approved business owner profile by userId
        // ═══════════════════════════════════════════════════════════
        private async Task<BusinessOwnerProfile> GetApprovedProfileAsync(string userId)
        {
            var spec = new BusinessOwnerProfileByUserIdSpecification(userId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec);

            if (profile == null)
                throw new NotFoundException("Business owner profile not found");

            if (profile.Status != ApprovalStatus.Approved)
                throw new ForbiddenException("Your business account is not approved yet");

            return profile;
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Get product and verify ownership
        // ═══════════════════════════════════════════════════════════
        private async Task<Product> GetOwnedProductAsync(int productId, int profileId)
        {
            var spec = new ProductByIdSpecification(productId);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.BusinessOwnerProfileId != profileId)
                throw new ForbiddenException("You do not own this product");

            return product;
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Map ProductFilterDto → ProductFilterParams
        // ═══════════════════════════════════════════════════════════
        private static ProductFilterParams MapToParams(ProductFilterDto filter) => new()
        {
            Search = filter.Search,
            Status = filter.Status,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize
        };

        // ═══════════════════════════════════════════════════════════
        // FR-BO-07: Get My Products
        // ═══════════════════════════════════════════════════════════
        public async Task<Pagination<ProductDto>> GetMyProductsAsync(
            string businessOwnerUserId,
            ProductFilterDto filter)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);
            var filterParams = MapToParams(filter);

            var countSpec = new ProductsByBusinessOwnerSpecification(profile.Id, filterParams, true);
            var totalCount = await _unitOfWork.Repository<Product>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new ProductsByBusinessOwnerSpecification(profile.Id, filterParams);
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<ProductDto>>(products);

            return new Pagination<ProductDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        // ═══════════════════════════════════════════════════════════
        // Get Single Product
        // ═══════════════════════════════════════════════════════════
        public async Task<ProductDto> GetProductByIdAsync(int productId, string businessOwnerUserId)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);
            var product = await GetOwnedProductAsync(productId, profile.Id);
            return _mapper.Map<ProductDto>(product);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-08: Create Product
        // ═══════════════════════════════════════════════════════════
        public async Task<ProductDto> CreateProductAsync(
            CreateProductDto dto,
            List<IFormFile> images,
            string businessOwnerUserId)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);

            // Validate images
            if (images == null || images.Count == 0)
                throw new BadRequestException("At least one product image is required");

            if (images.Count > 5)
                throw new BadRequestException("Maximum 5 images allowed");

            foreach (var img in images)
            {
                if (!_imageService.IsValidImage(img))
                    throw new BadRequestException(
                        $"Image '{img.FileName}' is invalid. Max 5MB, JPEG/PNG only");
            }

            // Create product entity
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Tags = dto.Tags,
                CategoryId = dto.CategoryId,
                BusinessOwnerProfileId = profile.Id,
                Status = ProductStatus.PendingApproval
            };

            _unitOfWork.Repository<Product>().Add(product);
            await _unitOfWork.CompleteAsync(); // need product.Id for images

            // Upload images
            for (int i = 0; i < images.Count; i++)
            {
                var imageUrl = await _imageService.UploadImageAsync(images[i], "products");
                _unitOfWork.Repository<ProductImage>().Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = imageUrl,
                    IsMain = i == 0,
                    SortOrder = i
                });
            }

            await _unitOfWork.CompleteAsync();
            // Notify AI to compute quality + demand for this product
            _ = Task.Run(() => _aiService.ComputeProductAsync(product.Id));
            return await GetProductByIdAsync(product.Id, businessOwnerUserId);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-09: Update Product
        // ═══════════════════════════════════════════════════════════
        public async Task<ProductDto> UpdateProductAsync(
            int productId,
            UpdateProductDto dto,
            List<IFormFile>? newImages,
            string businessOwnerUserId)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);
            var product = await GetOwnedProductAsync(productId, profile.Id);

            // FR-BO-09: Only Draft or Approved can be edited
            if (product.Status == ProductStatus.PendingApproval)
                throw new BadRequestException("Cannot edit a product that is pending approval");

            if (product.Status == ProductStatus.Rejected)
                throw new BadRequestException("Cannot edit a rejected product");

            // Update fields
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.Tags = dto.Tags;
            product.CategoryId = dto.CategoryId;

            // FR-BO-09: Editing approved product resets to pending
            if (product.Status == ProductStatus.Approved)
            {
                product.Status = ProductStatus.PendingApproval;
                product.ApprovedBy = null;
                product.ApprovedAt = null;
            }

            // Delete requested images
            if (dto.ImagesToDelete != null && dto.ImagesToDelete.Any())
            {
                foreach (var imgId in dto.ImagesToDelete)
                {
                    var img = product.Images.FirstOrDefault(i => i.Id == imgId);
                    if (img != null)
                    {
                        await _imageService.DeleteImageAsync(img.ImageUrl);
                        _unitOfWork.Repository<ProductImage>().Delete(img);
                    }
                }
            }

            // Add new images
            if (newImages != null && newImages.Any())
            {
                var deletedCount = dto.ImagesToDelete?.Count ?? 0;
                var remainingCount = product.Images.Count - deletedCount;

                if (remainingCount + newImages.Count > 5)
                    throw new BadRequestException(
                        $"Total images cannot exceed 5. You currently have {remainingCount} remaining");

                foreach (var img in newImages)
                {
                    if (!_imageService.IsValidImage(img))
                        throw new BadRequestException(
                            $"Image '{img.FileName}' is invalid. Max 5MB, JPEG/PNG only");

                    var url = await _imageService.UploadImageAsync(img, "products");
                    _unitOfWork.Repository<ProductImage>().Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = url,
                        IsMain = false,
                        SortOrder = product.Images.Count
                    });
                }
            }

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();
            // Notify AI to recompute product metrics
            // ✅ AI should compute quality + demand
            _ = Task.Run(async () =>
            {
                try
                {
                    await _aiService.ComputeProductAsync(product.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error computing product metrics for product {ProductId}", product.Id);
                }
            });
            return await GetProductByIdAsync(product.Id, businessOwnerUserId);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-10: Soft Delete Product
        // ═══════════════════════════════════════════════════════════
        public async Task DeleteProductAsync(int productId, string businessOwnerUserId)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);
            var product = await GetOwnedProductAsync(productId, profile.Id);

            // AuditInterceptor handles setting IsDeleted = true automatically
            _unitOfWork.Repository<Product>().Delete(product);
            await _unitOfWork.CompleteAsync();
        }

        // ═══════════════════════════════════════════════════════════
        // CUSTOMER PRODUCT DISCOVERY ENDPOINTS (BRANCH 1)
        // ═══════════════════════════════════════════════════════════

        public async Task<HomepageDto> GetHomepageDataAsync()
        {
            // 1. Get Categories
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);

            // 2. Get Featured Products
            var featuredSpec = new FeaturedProductsSpecification();
            var featuredProducts = await _unitOfWork.Repository<Product>().GetAllWithSpecificationsAsync(featuredSpec);
            var featuredDtos = _mapper.Map<List<CustomerProductDto>>(featuredProducts);

            // 3. Get Trending Products
            var trendingSpec = new TrendingProductsSpecification();
            var trendingProducts = await _unitOfWork.Repository<Product>().GetAllWithSpecificationsAsync(trendingSpec);
            var trendingDtos = _mapper.Map<List<CustomerProductDto>>(trendingProducts);

            return new HomepageDto
            {
                Categories = categoryDtos,
                FeaturedProducts = featuredDtos,
                TrendingProducts = trendingDtos
            };
        }

        public async Task<Pagination<CustomerProductDto>> SearchProductsAsync(CustomerProductFilterDto filter)
        {
            var filterParams = new CustomerProductFilterParams
            {
                Search = filter.Search,
                CategoryId = filter.CategoryId,
                BrandId = filter.BrandId,
                MinPrice = filter.MinPrice,
                MaxPrice = filter.MaxPrice,
                SortBy = filter.SortBy,
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize
            };

            var countSpec = new CustomerProductsSpecification(filterParams, true);
            var totalCount = await _unitOfWork.Repository<Product>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new CustomerProductsSpecification(filterParams);
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<CustomerProductDto>>(products);

            return new Pagination<CustomerProductDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        public async Task<CustomerProductDetailDto> GetPublicProductByIdAsync(int productId, string? userId = null)
        {
            var spec = new ProductByIdPublicSpecification(productId);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(spec);

            if (product == null)
                throw new NotFoundException("Product not found or not active");

            // Increment view count for trending engagement scoring
            product.ViewCount++;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            var dto = _mapper.Map<CustomerProductDetailDto>(product);

            // Fetch similar products (same category, excluding current product, top 6)
            var similarSpec = new SimilarProductsSpecification(product.CategoryId, product.Id);
            var similarProducts = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(similarSpec);

            dto.SimilarProducts = _mapper.Map<List<CustomerProductDto>>(similarProducts);

            if (!string.IsNullOrEmpty(userId))
            {
                _ = Task.Run(async () =>
                {
                    await _userInteractionService.LogInteractionAsync(
                        userId: userId,
                        userType: UserInteractionType.Customer,
                        itemId: productId,
                        itemType: UserInteractionItemType.Product,
                        actionType: UserInteractionActionType.View,
                        category: product.Category.Name,
                        quantity: 1,
                        price: product.Price
                    );
                });
            }


            return dto;
        }

        public async Task<Pagination<CustomerProductDto>> GetByCategoryAsync(int categoryId, CustomerProductFilterDto filter)
        {
            filter.CategoryId = categoryId;
            return await SearchProductsAsync(filter);
        }

        public async Task<List<string>> GetAutocompleteAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            var lowercaseQuery = query.ToLower();

            // Fetch approved & visible products matching query prefix/contains in name/tags
            var filterParams = new CustomerProductFilterParams
            {
                Search = lowercaseQuery,
                PageSize = 10
            };
            var spec = new CustomerProductsSpecification(filterParams);
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(spec);

            var suggestions = products
                .Select(p => p.Name)
                .Distinct()
                .Take(10)
                .ToList();

            return suggestions;
        }

        public async Task<Pagination<BrandListDto>> GetBrandsAsync(BrandFilterDto filter)
        {
            var filterParams = new BrandFilterParams
            {
                Search = filter.Search,
                Category = filter.Category,
                SortBy = filter.SortBy,
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize
            };

            var countSpec = new BrandsSpecification(filterParams, true);
            var totalCount = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new BrandsSpecification(filterParams);
            var brands = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<BrandListDto>>(brands);

            return new Pagination<BrandListDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        public async Task<BrandDetailDto> GetBrandByIdAsync(int brandProfileId)
        {
            var spec = new BrandByIdSpecification(brandProfileId);
            var brand = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec);

            if (brand == null || brand.Status != ApprovalStatus.Approved)
                throw new NotFoundException("Brand profile not found or not approved");

            var dto = _mapper.Map<BrandDetailDto>(brand);

            // Fetch top 6 products of this brand to populate initial list
            var productFilter = new CustomerProductFilterParams
            {
                BrandId = brandProfileId,
                PageSize = 6
            };
            var productSpec = new CustomerProductsSpecification(productFilter);
            var products = await _unitOfWork.Repository<Product>()
                .GetAllWithSpecificationsAsync(productSpec);

            dto.Products = _mapper.Map<List<CustomerProductDto>>(products);

            return dto;
        }

        public async Task<Pagination<CustomerProductDto>> GetBrandProductsAsync(int brandProfileId, CustomerProductFilterDto filter)
        {
            filter.BrandId = brandProfileId;
            return await SearchProductsAsync(filter);
        }

        public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }
    }
}