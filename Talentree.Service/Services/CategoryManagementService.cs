using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;
using Talentree.Core.Specifications.CategorySpecifications;

namespace Talentree.Service.Services
{
    /// <summary>
    /// FR-AD-31: Admin management of product categories and subcategories.
    /// </summary>
    public class CategoryManagementService : ICategoryManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryManagementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<List<PlatformCategoryDto>> GetAllCategoriesAsync()
        {
            // Fetch all non-deleted categories including subcategories
            var all = await _unitOfWork.Repository<Category>()
                .GetAllWithSpecificationsAsync(new AllCategoriesWithSubcategoriesSpecification());

            // Return only root-level categories; subcategories are nested within CategoryDto
            return _mapper.Map<List<PlatformCategoryDto>>(all.Where(c => c.ParentCategoryId == null).ToList());
        }

        /// <inheritdoc/>
        public async Task<PlatformCategoryDto> GetCategoryByIdAsync(int id)
        {
            var category = await LoadCategoryAsync(id);
            return _mapper.Map<PlatformCategoryDto>(category);
        }

        /// <inheritdoc/>
        public async Task<PlatformCategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto, string adminId)
        {
            var category = await LoadCategoryAsync(id);

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.IconUrl = dto.IconUrl;
            category.DisplayOrder = dto.DisplayOrder;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = adminId;

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PlatformCategoryDto>(category);
        }

        /// <inheritdoc/>
        public async Task<PlatformCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto dto, string adminId)
        {
            // Verify the parent exists
            var parent = await LoadCategoryAsync(dto.ParentCategoryId);

            var subCategory = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                IconUrl = dto.IconUrl,
                ParentCategoryId = dto.ParentCategoryId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminId
            };

            _unitOfWork.Repository<Category>().Add(subCategory);
            await _unitOfWork.CompleteAsync();

            // Reload with parent + subcategory tree
            return await GetCategoryByIdAsync(subCategory.Id);
        }

        /// <inheritdoc/>
        public async Task ReorderCategoriesAsync(ReorderCategoriesDto dto, string adminId)
        {
            var ids = dto.Items.Select(i => i.Id).ToList();

            var categories = (await _unitOfWork.Repository<Category>()
                .GetAllWithSpecificationsAsync(new CategoriesByIdsSpecification(ids)))
                .ToList();

            foreach (var item in dto.Items)
            {
                var cat = categories.FirstOrDefault(c => c.Id == item.Id);
                if (cat == null) continue;

                cat.DisplayOrder = item.NewDisplayOrder;
                cat.UpdatedAt = DateTime.UtcNow;
                cat.UpdatedBy = adminId;
                _unitOfWork.Repository<Category>().Update(cat);
            }

            await _unitOfWork.CompleteAsync();
        }

        /// <inheritdoc/>
        public async Task<PlatformCategoryDto> ToggleCategoryDisabledAsync(int id, string adminId)
        {
            var category = await LoadCategoryAsync(id);

            category.IsDisabled = !category.IsDisabled;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = adminId;

            _unitOfWork.Repository<Category>().Update(category);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PlatformCategoryDto>(category);
        }

        // ── Private helpers ──────────────────────────────────────────

        private async Task<Category> LoadCategoryAsync(int id)
        {
            var spec = new CategoryByIdWithSubcategoriesSpecification(id);
            return await _unitOfWork.Repository<Category>().GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Category #{id} not found.");
        }
    }
}
