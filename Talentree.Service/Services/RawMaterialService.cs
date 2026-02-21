using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Common;
using Talentree.Core.DTOs.RawMaterial;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Service.Contract;
using Talentree.Repository.Data;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Handles raw material browsing for Business Owners.
    /// Materials are automatically scoped to the BO's business category.
    /// </summary>
    public class RawMaterialService : IRawMaterialService
    {
        private readonly TalentreeDbContext _context;

        public RawMaterialService(TalentreeDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<PaginationDto<RawMaterialDto>> GetMaterialsAsync(
            string businessOwnerId, string? category, string? search, int pageIndex, int pageSize)
        {
            var profile = await _context.Set<BusinessOwnerProfile>()
                .FirstOrDefaultAsync(p => p.UserId == businessOwnerId)
                ?? throw new KeyNotFoundException("Business owner profile not found.");

            var query = _context.Set<RawMaterial>()
                .Include(m => m.Supplier)
                .Where(m => m.IsAvailable)
                .Where(m => m.Category.Contains(profile.BusinessCategory) || category != null);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(m => m.Category == category);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Name.Contains(search) || m.Description.Contains(search));

            var total = await query.CountAsync();
            var materials = await query
                .OrderBy(m => m.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationDto<RawMaterialDto>(pageIndex, pageSize, total, materials.Select(MapToDto).ToList());
        }

        /// <inheritdoc/>
        public async Task<RawMaterialDto> GetMaterialByIdAsync(int id)
        {
            var material = await _context.Set<RawMaterial>()
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsAvailable)
                ?? throw new KeyNotFoundException("Material not found.");

            return MapToDto(material);
        }

        // ── Private helpers ───────────────────────────────────────

        private static RawMaterialDto MapToDto(RawMaterial m) => new()
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
            SupplierName = m.Supplier?.Name ?? string.Empty
        };
    }
}
