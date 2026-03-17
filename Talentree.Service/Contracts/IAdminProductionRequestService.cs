using Talentree.Core.Enums;
using Talentree.Service.DTOs.BoProductionRequest;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Admin-side management of Business Owner production service requests.
    /// Controls the full Talentree workflow from review through to completion.
    /// </summary>
    public interface IAdminProductionRequestService
    {
        /// <summary>
        /// Returns a paginated list of all production requests across all BOs,
        /// optionally filtered by status. Used for the Admin inbox view.
        /// </summary>
        /// <param name="status">Optional status filter — null returns all statuses.</param>
        /// <param name="pageIndex">1-based page number.</param>
        /// <param name="pageSize">Number of records per page.</param>
        Task<Pagination<ProductionRequestSummaryDto>> GetAllRequestsAsync(
            BoProductionRequestStatus? status, int pageIndex, int pageSize);

        /// <summary>
        /// Returns the full detail of any production request by ID.
        /// No BusinessOwnerId scope — Admins can access any request.
        /// </summary>
        /// <param name="requestId">The request's primary key.</param>
        Task<ProductionRequestDetailDto> GetRequestByIdAsync(int requestId);

        /// <summary>
        /// Marks the request as <c>UnderReview</c>, signalling to the BO that Talentree is looking at it.
        /// Only allowed from <c>Submitted</c> status.
        /// </summary>
        /// <param name="requestId">The request's primary key.</param>
        /// <param name="adminId">The authenticated Admin's identity ID.</param>
        Task<ProductionRequestDetailDto> MarkUnderReviewAsync(int requestId, string adminId);

        /// <summary>
        /// Sends a price quote and estimated completion timeline to the BO.
        /// Transitions from <c>UnderReview</c> → <c>Quoted</c>.
        /// The BO must then either confirm or cancel.
        /// </summary>
        /// <param name="requestId">The request's primary key.</param>
        /// <param name="adminId">The authenticated Admin's identity ID.</param>
        /// <param name="dto">Quoted price, estimated completion date, and optional message to BO.</param>
        Task<ProductionRequestDetailDto> SendQuoteAsync(
            int requestId, string adminId, SendQuoteDto dto);

        /// <summary>
        /// Starts manufacturing on a confirmed request.
        /// Only allowed from <c>Confirmed</c> status (BO has already accepted the quote).
        /// </summary>
        /// <param name="requestId">The request's primary key.</param>
        /// <param name="adminId">The authenticated Admin's identity ID.</param>
        Task<ProductionRequestDetailDto> StartProductionAsync(int requestId, string adminId);

        /// <summary>
        /// Marks production as completed — goods are ready for the Business Owner.
        /// Only allowed from <c>InProduction</c> status.
        /// </summary>
        /// <param name="requestId">The request's primary key.</param>
        /// <param name="adminId">The authenticated Admin's identity ID.</param>
        /// <param name="dto">Optional closing notes for the BO.</param>
        Task<ProductionRequestDetailDto> CompleteRequestAsync(
            int requestId, string adminId, CompleteRequestDto dto);

        /// <summary>
        /// Rejects a production request with a mandatory reason.
        /// Not allowed after <c>InProduction</c> has started.
        /// The rejection reason is stored in <c>AdminNotes</c> and visible to the BO.
        /// </summary>
        /// <param name="requestId">The request's primary key.</param>
        /// <param name="adminId">The authenticated Admin's identity ID.</param>
        /// <param name="dto">Mandatory rejection reason.</param>
        Task<ProductionRequestDetailDto> RejectRequestAsync(
            int requestId, string adminId, RejectRequestDto dto);
    }
}