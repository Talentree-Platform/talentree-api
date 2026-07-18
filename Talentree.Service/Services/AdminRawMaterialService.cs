using AutoMapper;
using Microsoft.AspNetCore.Http;
using Talentree.Core;
using Talentree.Service.DTOs.Admin.RawMaterial;
using Talentree.Service.DTOs.Common;
using Talentree.Core.Entities;
using Talentree.Core.Repository.Contract;
using Talentree.Service.Contracts;
using Talentree.Core.Specifications.RawMaterial;
using Talentree.Core.Specifications.Supplier;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Admin operations for raw material management including
    /// CRUD, stock management, and image uploads.
    /// </summary>
    public class AdminRawMaterialService : IAdminRawMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public AdminRawMaterialService(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
        }

        /// <inheritdoc/>
        public async Task<Pagination<AdminRawMaterialDto>> GetMaterialsAsync(
            string? category, string? search, bool? isAvailable, int pageIndex, int pageSize)
        {
            var spec = new AdminRawMaterialSpec(category, search, isAvailable, pageIndex, pageSize);
            var countSpec = new AdminRawMaterialCountSpec(category, search, isAvailable);

            var materials = await _unitOfWork.Repository<RawMaterial>().GetAllWithSpecificationsAsync(spec);
            var total = await _unitOfWork.Repository<RawMaterial>().GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<AdminRawMaterialDto>>(materials);
            return new Pagination<AdminRawMaterialDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> GetMaterialByIdAsync(int id)
        {
            var spec = new AdminRawMaterialByIdSpec(id);
            var material = await _unitOfWork.Repository<RawMaterial>().GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            return _mapper.Map<AdminRawMaterialDto>(material);
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> CreateMaterialAsync(CreateRawMaterialDto dto)
        {
            var supplierSpec = new SupplierByIdActiveSpec(dto.SupplierId);
            var supplier = await _unitOfWork.Repository<Supplier>().GetByIdWithSpecificationsAsync(supplierSpec)
                ?? throw new KeyNotFoundException($"Active supplier #{dto.SupplierId} not found.");

            var material = _mapper.Map<RawMaterial>(dto);
            material.IsAvailable = dto.StockQuantity > 0; // Auto-set based on initial stock

            _unitOfWork.Repository<RawMaterial>().Add(material);
            await _unitOfWork.CompleteAsync();

            // Attach supplier so mapper can resolve SupplierName without a reload
            material.Supplier = supplier;
            return _mapper.Map<AdminRawMaterialDto>(material);
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> UpdateMaterialAsync(int id, UpdateRawMaterialDto dto)
        {
            var spec = new AdminRawMaterialByIdSpec(id);
            var material = await _unitOfWork.Repository<RawMaterial>().GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            if (dto.SupplierId.HasValue)
            {
                var supplierSpec = new SupplierByIdActiveSpec(dto.SupplierId.Value);
                var supplier = await _unitOfWork.Repository<Supplier>().GetByIdWithSpecificationsAsync(supplierSpec)
                    ?? throw new KeyNotFoundException($"Active supplier #{dto.SupplierId} not found.");
                material.SupplierId = dto.SupplierId.Value;
                material.Supplier = supplier;
            }

            if (dto.Name != null) material.Name = dto.Name;
            if (dto.Description != null) material.Description = dto.Description;
            if (dto.Price.HasValue) material.Price = dto.Price.Value;
            if (dto.Unit != null) material.Unit = dto.Unit;
            if (dto.MinimumOrderQuantity.HasValue) material.MinimumOrderQuantity = dto.MinimumOrderQuantity.Value;
            if (dto.StockQuantity.HasValue)
            {
                material.StockQuantity = dto.StockQuantity.Value;
                // Auto-mark unavailable when stock hits zero
                if (dto.StockQuantity.Value == 0) material.IsAvailable = false;
            }
            if (dto.Category != null) material.Category = dto.Category;
            if (dto.IsAvailable.HasValue) material.IsAvailable = dto.IsAvailable.Value;
            if (dto.PictureUrl != null) material.PictureUrl = dto.PictureUrl;

            _unitOfWork.Repository<RawMaterial>().Update(material);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<AdminRawMaterialDto>(material);
        }

        /// <inheritdoc/>
        public async Task DeleteMaterialAsync(int id)
        {
            var material = await _unitOfWork.Repository<RawMaterial>().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            // Soft delete — IsDeleted and DeletedAt are set by AuditInterceptor
            material.IsDeleted = true;
            material.IsAvailable = false;

            _unitOfWork.Repository<RawMaterial>().Update(material);
            await _unitOfWork.CompleteAsync();
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> RestockMaterialAsync(int id, RestockMaterialDto dto)
        {
            var spec = new AdminRawMaterialByIdSpec(id);
            var material = await _unitOfWork.Repository<RawMaterial>().GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            material.StockQuantity += dto.QuantityToAdd;
            material.IsAvailable = true; // Re-enable if it was out of stock

            _unitOfWork.Repository<RawMaterial>().Update(material);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<AdminRawMaterialDto>(material);
        }

        /// <inheritdoc/>
        public async Task<string> UploadMaterialImageAsync(int id, IFormFile image)
        {
            var material = await _unitOfWork.Repository<RawMaterial>().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            if (!_imageService.IsValidImage(image))
                throw new InvalidOperationException(
                    "Invalid image. Only JPG and PNG files under 5MB are accepted.");

            // Delete old image from Cloudinary if one exists
            if (!string.IsNullOrEmpty(material.PictureUrl))
                await _imageService.DeleteImageAsync(material.PictureUrl);

            // Upload new image to Cloudinary
            var imageUrl = await _imageService.UploadImageAsync(image, "materials");
            material.PictureUrl = imageUrl;

            _unitOfWork.Repository<RawMaterial>().Update(material);
            await _unitOfWork.CompleteAsync();

            return imageUrl;
        }
    }
}