using System;

namespace Talentree.Core.Entities
{
    public class CustomerCartItem : BaseEntity
    {
        public int CartId { get; set; }
        public CustomerCart Cart { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
