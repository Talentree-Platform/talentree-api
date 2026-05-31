using System.Collections.Generic;

namespace Talentree.Service.DTOs.Customer
{
    public class WishlistDto
    {
        public List<WishlistItemDto> Items { get; set; } = new();
    }
}
