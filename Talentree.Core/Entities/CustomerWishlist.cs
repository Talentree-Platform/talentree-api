using System.Collections.Generic;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class CustomerWishlist : AuditableEntity
    {
        public string CustomerId { get; set; } = null!;
        public AppUser Customer { get; set; } = null!;
        public ICollection<CustomerWishlistItem> Items { get; set; } = new List<CustomerWishlistItem>();
    }
}
