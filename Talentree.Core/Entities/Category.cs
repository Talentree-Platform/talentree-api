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
        public ICollection<Product> Products { get; set; } = new List<Product>();

        // From ISoftDelete:
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

    }
}
