using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.BoProductionRequests;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.BoProductionRequest;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Admin-side management of BO production service requests.
    /// Controls the full Talentree workflow: UnderReview → Quoted → InProduction → Completed.
    /// Every transition appends an immutable history entry.
    /// </summary>
    public class AdminProductionRequestService : IAdminProductionRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminProductionRequestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<Pagination<ProductionRequestSummaryDto>> GetAllRequestsAsync(
            BoProductionRequestStatus? status, int pageIndex, int pageSize)
        {
            var listSpec = new AllProductionRequestsSpecification(status, pageIndex, pageSize);
            var countSpec = new AllProductionRequestsCountSpecification(status);

            var requests = await _unitOfWork.Repository<BoProductionRequest>()
                .GetAllWithSpecificationsAsync(listSpec);
            var total = await _unitOfWork.Repository<BoProductionRequest>()
                .GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<ProductionRequestSummaryDto>>(requests);
            return new Pagination<ProductionRequestSummaryDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> GetRequestByIdAsync(int requestId)
        {
            var spec = new ProductionRequestByIdForAdminSpecification(requestId);
            var request = await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Production request #{requestId} not found.");

            return _mapper.Map<ProductionRequestDetailDto>(request);
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> MarkUnderReviewAsync(int requestId, string adminId)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, BoProductionRequestStatus.Submitted);

            return await ApplyTransitionAsync(request, BoProductionRequestStatus.UnderReview,
                adminId, "Request taken under review by Talentree.");
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> SendQuoteAsync(
            int requestId, string adminId, SendQuoteDto dto)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, BoProductionRequestStatus.UnderReview);

            request.QuotedPrice = dto.QuotedPrice;
            request.EstimatedCompletionDate = dto.EstimatedCompletionDate;
            request.AdminNotes = dto.AdminNotes;

            return await ApplyTransitionAsync(request, BoProductionRequestStatus.Quoted, adminId,
                $"Quote sent: {dto.QuotedPrice:C}. Estimated completion: {dto.EstimatedCompletionDate:d}.");
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> StartProductionAsync(int requestId, string adminId)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, BoProductionRequestStatus.Confirmed);

            return await ApplyTransitionAsync(request, BoProductionRequestStatus.InProduction,
                adminId, "Production started by Talentree.");
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> CompleteRequestAsync(
            int requestId, string adminId, CompleteRequestDto dto)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, BoProductionRequestStatus.InProduction);

            request.CompletedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.AdminNotes))
                request.AdminNotes = dto.AdminNotes;

            return await ApplyTransitionAsync(request, BoProductionRequestStatus.Completed,
                adminId, "Production completed. Goods are ready for the business owner.");
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> RejectRequestAsync(
            int requestId, string adminId, RejectRequestDto dto)
        {
            var request = await LoadRequestAsync(requestId);

            if (request.Status is BoProductionRequestStatus.InProduction
                                or BoProductionRequestStatus.Completed
                                or BoProductionRequestStatus.Cancelled
                                or BoProductionRequestStatus.Rejected)
            {
                throw new InvalidOperationException(
                    $"Cannot reject a request with status '{request.Status}'.");
            }

            request.AdminNotes = dto.Reason;

            return await ApplyTransitionAsync(request, BoProductionRequestStatus.Rejected,
                adminId, $"Request rejected: {dto.Reason}");
        }

        // ── Private helpers ───────────────────────────────────────

        /// <summary>
        /// Loads a production request with full includes for Admin use.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        private async Task<BoProductionRequest> LoadRequestAsync(int requestId)
        {
            var spec = new ProductionRequestByIdForAdminSpecification(requestId);
            return await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Production request #{requestId} not found.");
        }

        /// <summary>
        /// Asserts the request is in <paramref name="expected"/> status before a transition.
        /// Throws <see cref="InvalidOperationException"/> if the status does not match.
        /// </summary>
        private static void AssertStatus(BoProductionRequest request, BoProductionRequestStatus expected)
        {
            if (request.Status != expected)
                throw new InvalidOperationException(
                    $"Expected status '{expected}' but current status is '{request.Status}'.");
        }

        /// <summary>
        /// Applies a status transition, appends an immutable history entry,
        /// persists all changes, then returns the refreshed detail DTO.
        /// </summary>
        private async Task<ProductionRequestDetailDto> ApplyTransitionAsync(
            BoProductionRequest request,
            BoProductionRequestStatus newStatus,
            string adminId,
            string notes)
        {
            request.Status = newStatus;
            request.StatusHistory.Add(new BoProductionRequestStatusHistory
            {
                Status = newStatus,
                ChangedByUserId = adminId,
                Notes = notes
            });

            await _unitOfWork.CompleteAsync();
            return await GetRequestByIdAsync(request.Id);
        }
    }
}
