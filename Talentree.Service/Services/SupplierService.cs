using AutoMapper;
using Talentree.Core;
using Talentree.Service.DTOs.Admin.Supplier;
using Talentree.Service.DTOs.Common;
using Talentree.Core.Entities;
using Talentree.Core.Repository.Contract;
using Talentree.Service.Contracts;
using Talentree.Core.Specifications.Supplier;

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

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
    }
}
