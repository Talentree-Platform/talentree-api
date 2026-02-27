using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Talentree.Core;
using Talentree.Core.DTOs.Admin.RawMaterial;
using Talentree.Core.DTOs.Common;
using Talentree.Core.Entities;
using Talentree.Core.Repository.Contract;
using Talentree.Core.Service.Contract;
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
        private readonly IWebHostEnvironment _env;

        public AdminRawMaterialService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _env = env;
        }

        /// <inheritdoc/>
        public async Task<PaginationDto<AdminRawMaterialDto>> GetMaterialsAsync(
            string? category, string? search, bool? isAvailable, int pageIndex, int pageSize)
        {
            var spec = new AdminRawMaterialSpec(category, search, isAvailable, pageIndex, pageSize);
            var countSpec = new AdminRawMaterialCountSpec(category, search, isAvailable);

            var materials = await _unitOfWork.Repository<RawMaterial>().GetAllWithSpecificationsAsync(spec);
            var total = await _unitOfWork.Repository<RawMaterial>().GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<AdminRawMaterialDto>>(materials);
            return new PaginationDto<AdminRawMaterialDto>(pageIndex, pageSize, total, dtos);
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

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only JPG, PNG and WebP images are allowed.");

            if (image.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Image must be under 5MB.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "materials");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"material_{id}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
                await image.CopyToAsync(stream);

            // Delete old image file if one exists
            if (!string.IsNullOrEmpty(material.PictureUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, material.PictureUrl.TrimStart('/'));
                if (File.Exists(oldPath)) File.Delete(oldPath);
            }

            var relativeUrl = $"/images/materials/{fileName}";
            material.PictureUrl = relativeUrl;

            _unitOfWork.Repository<RawMaterial>().Update(material);
            await _unitOfWork.CompleteAsync();

            return relativeUrl;
        }
    }
}