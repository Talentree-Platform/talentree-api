using System;

namespace Talentree.Core.Entities
{
    public class CustomerWishlistItem : BaseEntity
    {
        public int WishlistId { get; set; }
        public CustomerWishlist Wishlist { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
