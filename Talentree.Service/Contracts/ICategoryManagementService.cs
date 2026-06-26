using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// FR-AD-31: Admin management of product categories and subcategories.
    /// </summary>
    public interface ICategoryManagementService
    {
        /// <summary>Returns all categories as a hierarchical tree (parent → subcategories).</summary>
        Task<List<PlatformCategoryDto>> GetAllCategoriesAsync();

        /// <summary>Returns a single category by ID including its subcategories.</summary>
        Task<PlatformCategoryDto> GetCategoryByIdAsync(int id);

        /// <summary>Updates a category's name, description, icon, and display order.</summary>
        Task<PlatformCategoryDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto, string adminId);

        /// <summary>Creates a new subcategory under the specified parent category.</summary>
        Task<PlatformCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto dto, string adminId);

        /// <summary>Applies a batch display-order update to multiple categories.</summary>
        Task ReorderCategoriesAsync(ReorderCategoriesDto dto, string adminId);

        /// <summary>
        /// Toggles the IsDisabled flag on a category.
        /// Disabled categories are hidden from users but data (including products) is preserved.
        /// </summary>
        Task<PlatformCategoryDto> ToggleCategoryDisabledAsync(int id, string adminId);
    }
}
