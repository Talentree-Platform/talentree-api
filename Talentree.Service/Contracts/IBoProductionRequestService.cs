using Talentree.Service.DTOs.BoProductionRequest;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Manages production service requests submitted by Business Owners to Talentree.
    /// A production request is a manufacturing commission — the BO describes what they
    /// want produced, Talentree reviews it, sends a price quote, and starts production
    /// only after the BO explicitly confirms the quote.
    /// </summary>
    public interface IBoProductionRequestService
    {
        /// <summary>
        /// Submits a new production request to Talentree.
        /// Validates any referenced raw material IDs, then creates the request in
        /// <c>Submitted</c> status and appends the first history entry.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="dto">The production request form data.</param>
        Task<ProductionRequestDetailDto> SubmitRequestAsync(
            string businessOwnerId, SubmitProductionRequestDto dto);

        /// <summary>
        /// Returns a paginated list of the BO's own production requests, newest first.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="pageIndex">1-based page number.</param>
        /// <param name="pageSize">Number of records per page.</param>
        Task<Pagination<ProductionRequestSummaryDto>> GetMyRequestsAsync(
            string businessOwnerId, int pageIndex, int pageSize);

        /// <summary>
        /// Returns the full detail of a single production request including status history.
        /// Scoped to the BO — a BO cannot retrieve another BO's request.
        /// Throws <see cref="KeyNotFoundException"/> if not found or not owned by the BO.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="requestId">The request's primary key.</param>
        Task<ProductionRequestDetailDto> GetRequestByIdAsync(
            string businessOwnerId, int requestId);

        /// <summary>
        /// Cancels the BO's own production request.
        /// Only allowed while the request is in <c>Submitted</c> or <c>Quoted</c> status.
        /// Throws <see cref="InvalidOperationException"/> if cancellation is not permitted.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="requestId">The request's primary key.</param>
        Task<ProductionRequestDetailDto> CancelRequestAsync(
            string businessOwnerId, int requestId);

        /// <summary>
        /// Confirms Talentree's price quote, authorising production to begin.
        /// Only allowed when the request is in <c>Quoted</c> status.
        /// Throws <see cref="InvalidOperationException"/> if the request is not in Quoted status.
        /// </summary>
        /// <param name="businessOwnerId">The authenticated BO's identity ID.</param>
        /// <param name="requestId">The request's primary key.</param>
        Task<ProductionRequestDetailDto> ConfirmQuoteAsync(
            string businessOwnerId, int requestId);
    }
}