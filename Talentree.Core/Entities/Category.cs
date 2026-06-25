using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Core.Entities
{
    public class Category : AuditableEntity, ISoftDelete
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // AI Team requirement - maps to business type
        public string? BusinessType { get; set; }

        // FR-AD-31: Category Management
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }

        /// <summary>
        /// When true, this category is hidden from users but data is preserved.
        /// Uses a dedicated flag (not ISoftDelete) so the record is never fully removed.
        /// </summary>
        public bool IsDisabled { get; set; }

        // Self-referencing FK for subcategories (Phase 1)
        public int? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

        // From ISoftDelete:
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

    }
}
