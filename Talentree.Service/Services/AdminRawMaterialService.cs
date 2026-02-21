using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Admin.RawMaterial;
using Talentree.Core.DTOs.Common;
using Talentree.Core.Entities;
using Talentree.Core.Service.Contract;
using Talentree.Repository.Data;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Handles admin operations for raw material management including
    /// CRUD, stock management, and image uploads.
    /// </summary>
    public class AdminRawMaterialService : IAdminRawMaterialService
    {
        private readonly TalentreeDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminRawMaterialService(TalentreeDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <inheritdoc/>
        public async Task<PaginationDto<AdminRawMaterialDto>> GetMaterialsAsync(
            string? category, string? search, bool? isAvailable, int pageIndex, int pageSize)
        {
            // Admin sees ALL materials including unavailable ones.
            // IsAvailable is a business flag — IsDeleted is the soft-delete filter applied globally.
            var query = _context.Set<RawMaterial>()
                .Include(m => m.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(m => m.Category.Contains(category));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Name.Contains(search) || m.Description.Contains(search));

            if (isAvailable.HasValue)
                query = query.Where(m => m.IsAvailable == isAvailable.Value);

            var total = await query.CountAsync();
            var materials = await query
                .OrderBy(m => m.Category).ThenBy(m => m.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationDto<AdminRawMaterialDto>(
                pageIndex, pageSize, total, materials.Select(MapToDto).ToList());
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> GetMaterialByIdAsync(int id)
        {
            var material = await _context.Set<RawMaterial>()
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            return MapToDto(material);
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> CreateMaterialAsync(CreateRawMaterialDto dto)
        {
            var supplier = await _context.Set<Supplier>()
                .FirstOrDefaultAsync(s => s.Id == dto.SupplierId && s.IsActive)
                ?? throw new KeyNotFoundException($"Active supplier #{dto.SupplierId} not found.");

            var material = new RawMaterial
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Unit = dto.Unit,
                MinimumOrderQuantity = dto.MinimumOrderQuantity,
                StockQuantity = dto.StockQuantity,
                Category = dto.Category,
                SupplierId = dto.SupplierId,
                PictureUrl = dto.PictureUrl,
                IsAvailable = dto.StockQuantity > 0 // Auto-set based on initial stock
            };

            _context.Set<RawMaterial>().Add(material);
            await _context.SaveChangesAsync();

            // Attach navigation property so MapToDto has supplier name without a reload
            material.Supplier = supplier;
            return MapToDto(material);
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> UpdateMaterialAsync(int id, UpdateRawMaterialDto dto)
        {
            var material = await _context.Set<RawMaterial>()
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            if (dto.SupplierId.HasValue)
            {
                var supplier = await _context.Set<Supplier>()
                    .FirstOrDefaultAsync(s => s.Id == dto.SupplierId.Value && s.IsActive)
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

            await _context.SaveChangesAsync();
            return MapToDto(material);
        }

        /// <inheritdoc/>
        public async Task DeleteMaterialAsync(int id)
        {
            var material = await _context.Set<RawMaterial>().FindAsync(id)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            // Soft delete — IsDeleted and DeletedAt are set by AuditInterceptor
            material.IsDeleted = true;
            material.IsAvailable = false;
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<AdminRawMaterialDto> RestockMaterialAsync(int id, RestockMaterialDto dto)
        {
            var material = await _context.Set<RawMaterial>()
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new KeyNotFoundException($"Material #{id} not found.");

            material.StockQuantity += dto.QuantityToAdd;
            material.IsAvailable = true; // Re-enable if it was out of stock

            await _context.SaveChangesAsync();
            return MapToDto(material);
        }

        /// <inheritdoc/>
        public async Task<string> UploadMaterialImageAsync(int id, IFormFile image)
        {
            var material = await _context.Set<RawMaterial>().FindAsync(id)
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
            await _context.SaveChangesAsync();

            return relativeUrl;
        }

        // ── Private helpers ───────────────────────────────────────

        private static AdminRawMaterialDto MapToDto(RawMaterial m) => new()
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            Price = m.Price,
            Unit = m.Unit,
            MinimumOrderQuantity = m.MinimumOrderQuantity,
            StockQuantity = m.StockQuantity,
            IsAvailable = m.IsAvailable,
            Category = m.Category,
            PictureUrl = m.PictureUrl,
            SupplierId = m.SupplierId,
            SupplierName = m.Supplier?.Name ?? string.Empty,
            CreatedAt = m.CreatedAt
        };
    }
}
