using System.Threading.Tasks;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.Contracts
{
    public interface IWishlistService
    {
        Task<WishlistDto> GetWishlistAsync(string customerId);
        Task<WishlistDto> AddToWishlistAsync(int productId, string customerId);
        Task<WishlistDto> RemoveFromWishlistAsync(int productId, string customerId);
        Task<CartDto> MoveAllToCartAsync(string customerId);
        Task<bool> IsWishlistedAsync(int productId, string customerId);
    }
}
