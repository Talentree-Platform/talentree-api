namespace Talentree.Service.DTOs.PlatformSettings
{
    // ═══════════════════════════════════════════════════════════
    // FR-AD-31: Category Management DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>Full category details including subcategory tree.</summary>
    public class PlatformCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BusinessType { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDisabled { get; set; }
        public int? ParentCategoryId { get; set; }
        public List<PlatformCategoryDto> SubCategories { get; set; } = new();
    }

    /// <summary>Lightweight category summary for lists.</summary>
    public class PlatformCategorySummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDisabled { get; set; }
        public int? ParentCategoryId { get; set; }
        public int SubCategoryCount { get; set; }
    }

    /// <summary>Request to update an existing category's metadata.</summary>
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>Request to create a subcategory under a parent category.</summary>
    public class CreateSubCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int ParentCategoryId { get; set; }
    }

    /// <summary>Single item in a reorder batch — maps a category Id to its new display order.</summary>
    public class CategoryOrderItemDto
    {
        public int Id { get; set; }
        public int NewDisplayOrder { get; set; }
    }

    /// <summary>Batch reorder request for categories.</summary>
    public class ReorderCategoriesDto
    {
        public List<CategoryOrderItemDto> Items { get; set; } = new();
    }
}
