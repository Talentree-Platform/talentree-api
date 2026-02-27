using AutoMapper;
using Talentree.Core;
using Talentree.Core.DTOs.Common;
using Talentree.Core.DTOs.RawMaterial;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Repository.Contract;
using Talentree.Core.Service.Contract;
using Talentree.Core.Specifications.Identity;
using Talentree.Core.Specifications.RawMaterial;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Handles raw material browsing for Business Owners.
    /// Materials are automatically scoped to the BO's business category.
    /// </summary>
    public class RawMaterialService : IRawMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RawMaterialService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<PaginationDto<RawMaterialDto>> GetMaterialsAsync(
            string businessOwnerId, string? category, string? search, int pageIndex, int pageSize)
        {
            // Resolve BO's business category first — used to scope material results
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(new BusinessOwnerProfileByUserIdSpec(businessOwnerId))
                ?? throw new KeyNotFoundException("Business owner profile not found.");

            var spec = new RawMaterialWithSupplierSpec(profile.BusinessCategory, category, search, pageIndex, pageSize);
            var countSpec = new RawMaterialCountSpec(profile.BusinessCategory, category, search);

            var materials = await _unitOfWork.Repository<RawMaterial>().GetAllWithSpecificationsAsync(spec);
            var total = await _unitOfWork.Repository<RawMaterial>().GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<RawMaterialDto>>(materials);
            return new PaginationDto<RawMaterialDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<RawMaterialDto> GetMaterialByIdAsync(int id)
        {
            var spec = new RawMaterialByIdWithSupplierSpec(id);
            var material = await _unitOfWork.Repository<RawMaterial>().GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException("Material not found.");

            return _mapper.Map<RawMaterialDto>(material);
        }
    }
}