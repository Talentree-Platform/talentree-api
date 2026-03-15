using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.MaterialOrder;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Handles checkout and order history for Business Owner raw-material purchases.
    /// A material order is created when a BO submits their basket through checkout.
    /// Prices are locked and stock is deducted atomically at that point.
    /// </summary>
    public interface IMaterialOrderService
    {
        /// <summary>
        /// Creates a material order from the BO's current basket.
        /// In a single transaction: validates stock, locks prices into order lines,
        /// deducts stock from the catalogue, and clears the basket.
        /// Throws if the basket is empty or any item's stock has become insufficient.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="dto">Delivery address and payment details.</param>
        Task<MaterialOrderDto> CheckoutAsync(string businessOwnerId, MaterialCheckoutDto dto);

        /// <summary>
        /// Returns a paginated list of the BO's past material orders, newest first.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="pageIndex">1-based page number.</param>
        /// <param name="pageSize">Number of records per page.</param>
        Task<Pagination<MaterialOrderSummaryDto>> GetOrderHistoryAsync(
            string businessOwnerId, int pageIndex, int pageSize);

        /// <summary>
        /// Returns the full detail of a single material order including all line items.
        /// Scoped to the BO — a BO cannot retrieve another BO's order.
        /// Throws <see cref="KeyNotFoundException"/> if not found or not owned by the BO.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="orderId">The order's primary key.</param>
        Task<MaterialOrderDto> GetOrderByIdAsync(string businessOwnerId, int orderId);
    }
}