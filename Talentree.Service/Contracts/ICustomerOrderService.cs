using System.Threading.Tasks;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Customer;
using Talentree.Core.Enums;

namespace Talentree.Service.Contracts
{
    public interface ICustomerOrderService
    {
        // Checkout
        Task<CustomerOrderDetailDto> PlaceOrderAsync(CheckoutDeliveryDto delivery, PaymentMethod method, string customerId);
        Task<OrderPaymentDto> CreatePaymentIntentAsync(int orderId, string customerId);

        // Order history & details
        Task<Pagination<CustomerOrderSummaryDto>> GetOrdersAsync(string customerId, OrderFilterDto filter);
        Task<CustomerOrderDetailDto> GetOrderByIdAsync(int orderId, string customerId);

        // Actions
        Task CancelOrderAsync(int orderId, string customerId);
        Task<byte[]> GenerateInvoiceAsync(int orderId, string customerId);
    }
}
