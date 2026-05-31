using System.Threading.Tasks;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.Contracts
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string customerId);
        Task<CartDto> AddToCartAsync(AddToCartDto dto, string customerId);
        Task<CartDto> UpdateCartItemAsync(int productId, UpdateCartItemDto dto, string customerId);
        Task<CartDto> RemoveFromCartAsync(int productId, string customerId);
        Task ClearCartAsync(string customerId);
        Task<CheckoutSummaryDto> PreviewCheckoutAsync(string customerId, CheckoutDeliveryDto delivery);
    }
}
