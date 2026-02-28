using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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

namespace Talentree.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public ProductService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
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
    }
}