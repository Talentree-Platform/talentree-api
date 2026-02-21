using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.DTOs.Admin.Supplier;
using Talentree.Core.DTOs.Common;
using Talentree.Core.Entities;
using Talentree.Core.Service.Contract;
using Talentree.Repository.Data;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Handles all supplier management operations for the admin panel.
    /// Suppliers are static entities managed by admins — they have no user accounts.
    /// </summary>
    public class SupplierService : ISupplierService
    {
        private readonly TalentreeDbContext _context;

        public SupplierService(TalentreeDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<PaginationDto<SupplierDto>> GetSuppliersAsync(
            string? search, bool? isActive, int pageIndex, int pageSize)
        {
            var query = _context.Set<Supplier>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Name.Contains(search) || s.ContactPerson.Contains(search));

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            var total = await query.CountAsync();
            var suppliers = await query
                .OrderBy(s => s.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address,
                    City = s.City,
                    Country = s.Country,
                    ContactPerson = s.ContactPerson,
                    TaxId = s.TaxId,
                    IsActive = s.IsActive,
                    MaterialCount = s.RawMaterials.Count(m => !m.IsDeleted)
                })
                .ToListAsync();

            return new PaginationDto<SupplierDto>(pageIndex, pageSize, total, suppliers);
        }

        /// <inheritdoc/>
        public async Task<SupplierDto> GetSupplierByIdAsync(int id)
        {
            var supplier = await _context.Set<Supplier>()
                .Include(s => s.RawMaterials)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Supplier #{id} not found.");

            return MapToDto(supplier);
        }

        /// <inheritdoc/>
        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
        {
            var exists = await _context.Set<Supplier>()
                .AnyAsync(s => s.Email == dto.Email);

            if (exists)
                throw new InvalidOperationException($"A supplier with email '{dto.Email}' already exists.");

            var supplier = new Supplier
            {
                Name = dto.Name,
                Description = dto.Description,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                ContactPerson = dto.ContactPerson,
                TaxId = dto.TaxId,
                IsActive = true
            };

            _context.Set<Supplier>().Add(supplier);
            await _context.SaveChangesAsync();

            return MapToDto(supplier);
        }

        /// <inheritdoc/>
        public async Task<SupplierDto> UpdateSupplierAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await _context.Set<Supplier>().FindAsync(id)
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

            await _context.SaveChangesAsync();
            return MapToDto(supplier);
        }

        /// <inheritdoc/>
        public async Task DeleteSupplierAsync(int id)
        {
            var supplier = await _context.Set<Supplier>()
                .Include(s => s.RawMaterials)
                .FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new KeyNotFoundException($"Supplier #{id} not found.");

            var hasActiveMaterials = supplier.RawMaterials.Any(m => m.IsAvailable && !m.IsDeleted);
            if (hasActiveMaterials)
                throw new InvalidOperationException(
                    "Cannot delete supplier with active materials. Deactivate all their materials first.");

            // Soft delete — IsDeleted and DeletedAt are set by AuditInterceptor
            supplier.IsDeleted = true;
            supplier.IsActive = false;
            await _context.SaveChangesAsync();
        }

        // ── Private helpers ───────────────────────────────────────

        private static SupplierDto MapToDto(Supplier s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address,
            City = s.City,
            Country = s.Country,
            ContactPerson = s.ContactPerson,
            TaxId = s.TaxId,
            IsActive = s.IsActive,
            MaterialCount = s.RawMaterials?.Count(m => !m.IsDeleted) ?? 0
        };
    }
}
