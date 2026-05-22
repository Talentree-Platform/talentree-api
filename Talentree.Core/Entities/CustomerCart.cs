using System.Collections.Generic;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class CustomerCart : AuditableEntity
    {
        public string CustomerId { get; set; } = null!;       // AppUser FK
        public AppUser Customer { get; set; } = null!;
        public ICollection<CustomerCartItem> Items { get; set; } = new List<CustomerCartItem>();
    }
}
