using AutoMapper;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.BoProductionRequests;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.BoProductionRequest;
using Talentree.Service.DTOs.Common;
using Talentree.Core.Entities.Identity;
using Talentree.Service.Messaging;
using Talentree.Service.Messaging.Contracts;

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
        private readonly INotificationHelperService _notificationHelper; 
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AdminProductionRequestService> _logger;

        public AdminProductionRequestService(IUnitOfWork unitOfWork, IMapper mapper, INotificationHelperService notificationHelper, ILogger<AdminProductionRequestService> logger, IEventPublisher eventPublisher)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationHelper = notificationHelper;
            _logger = logger;
            _eventPublisher = eventPublisher;
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
            await EnrichSummaryDtosAsync(dtos, requests);
            return new Pagination<ProductionRequestSummaryDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> GetRequestByIdAsync(int requestId)
        {
            var spec = new ProductionRequestByIdForAdminSpecification(requestId);
            var request = await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Production request #{requestId} not found.");

            var dto = _mapper.Map<ProductionRequestDetailDto>(request);
            await EnrichDetailDtoAsync(dto, request.BusinessOwnerId);
            return dto;
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> MarkUnderReviewAsync(int requestId, string adminId)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, BoProductionRequestStatus.Submitted);

            var result = await ApplyTransitionAsync(request, BoProductionRequestStatus.UnderReview,
                 adminId, "Request taken under review by Talentree.");

            await _notificationHelper.NotifyProductionRequestCreated(requestId, request.BusinessOwnerId);
            _logger.LogInformation("Production request {RequestId} marked as under review", requestId);

            return result;
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
            var result = await ApplyTransitionAsync(request, BoProductionRequestStatus.Quoted, adminId,
                 $"Quote sent: {dto.QuotedPrice:C}. Estimated completion: {dto.EstimatedCompletionDate:d}.");

            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyProductionQuoteSent(requestId, request.BusinessOwnerId, dto.QuotedPrice);
            _logger.LogInformation("Quote sent for production request {RequestId}: {Price}", requestId, dto.QuotedPrice);

            return result;
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> StartProductionAsync(int requestId, string adminId)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, BoProductionRequestStatus.Confirmed);
            var result = await ApplyTransitionAsync(request, BoProductionRequestStatus.InProduction,
                adminId, "Production started by Talentree.");

            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyProductionStarted(requestId, request.BusinessOwnerId);
            _logger.LogInformation("Production started for request {RequestId}", requestId);

            return result;
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

            var result = await ApplyTransitionAsync(request, BoProductionRequestStatus.Completed,
                 adminId, "Production completed. Goods are ready for the business owner.");

            // Publish AI completion request
            await _eventPublisher.PublishAsync("ai.request", new RequestComputationMessage { RequestId = requestId });

            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyProductionCompleted(requestId, request.BusinessOwnerId);
            _logger.LogInformation("Production completed for request {RequestId}", requestId);

            return result;
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


            var result = await ApplyTransitionAsync(request, BoProductionRequestStatus.Rejected,
                adminId, $"Request rejected: {dto.Reason}");

            // ✅ ADD NOTIFICATION
            await _notificationHelper.NotifyProductionRejected(requestId, request.BusinessOwnerId, dto.Reason);
            _logger.LogInformation("Production request {RequestId} rejected: {Reason}", requestId, dto.Reason);

            return result;
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

        // ═══════════════════════════════════════════════════════════
        // FR-AD-12: Service Request Management — Assignment & Notes
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> AssignRequestAsync(
            int requestId, string assignedAdminId, string actingAdminId)
        {
            var request = await LoadRequestAsync(requestId);

            if (request.Status is BoProductionRequestStatus.Completed
                                or BoProductionRequestStatus.Cancelled
                                or BoProductionRequestStatus.Rejected)
                throw new InvalidOperationException(
                    $"Cannot assign a request with status '{request.Status}'.");

            request.AssignedAdminId = assignedAdminId;
            request.UpdatedAt = DateTime.UtcNow;
            request.UpdatedBy = actingAdminId;

            _unitOfWork.Repository<BoProductionRequest>().Update(request);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Production request {RequestId} assigned to admin {AssignedAdminId} by {ActingAdminId}.",
                requestId, assignedAdminId, actingAdminId);

            return await GetRequestByIdAsync(requestId);
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> AddNoteAsync(
            int requestId, string note, string adminId)
        {
            if (string.IsNullOrWhiteSpace(note))
                throw new InvalidOperationException("Note cannot be empty.");

            var request = await LoadRequestAsync(requestId);

            // Prefix with timestamp so the conversation thread is traceable
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC");
            var prefixedNote = $"[{timestamp}] {note}";

            request.AdminNotes = string.IsNullOrWhiteSpace(request.AdminNotes)
                ? prefixedNote
                : request.AdminNotes + "\n\n" + prefixedNote;

            request.UpdatedAt = DateTime.UtcNow;
            request.UpdatedBy = adminId;

            _unitOfWork.Repository<BoProductionRequest>().Update(request);
            await _unitOfWork.CompleteAsync();

            // Notify the seller
            await _notificationHelper.NotifyAllAdmins(
                title: $"Note Added to Request #{requestId}",
                message: note,
                type: Core.Enums.NotificationType.Order,
                actionUrl: $"/admin/production-requests/{requestId}"
            );

            _logger.LogInformation("Note added to production request {RequestId} by admin {AdminId}.", requestId, adminId);

            return await GetRequestByIdAsync(requestId);
        }

        private async Task EnrichDetailDtoAsync(ProductionRequestDetailDto dto, string businessOwnerId)
        {
            var user = await _unitOfWork.Repository<AppUser>().GetByIdAsync(businessOwnerId);
            if (user != null)
            {
                dto.BusinessOwnerName = user.DisplayName;
                var profiles = await _unitOfWork.Repository<BusinessOwnerProfile>()
                    .FindAsync(p => p.UserId == businessOwnerId);
                var profile = profiles.FirstOrDefault();
                if (profile != null)
                {
                    dto.BusinessName = profile.BusinessName;
                }
            }
        }

        private async Task EnrichSummaryDtosAsync(List<ProductionRequestSummaryDto> dtos, IReadOnlyList<BoProductionRequest> requests)
        {
            var distinctIds = requests.Select(r => r.BusinessOwnerId).Distinct().ToList();
            if (!distinctIds.Any()) return;

            var users = await _unitOfWork.Repository<AppUser>().FindAsync(u => distinctIds.Contains(u.Id));
            var profiles = await _unitOfWork.Repository<BusinessOwnerProfile>().FindAsync(p => distinctIds.Contains(p.UserId));

            var userMap = users.ToDictionary(u => u.Id, u => u.DisplayName);
            var profileMap = profiles.ToDictionary(p => p.UserId, p => p.BusinessName);

            for (int i = 0; i < dtos.Count; i++)
            {
                var request = requests[i];
                var dto = dtos[i];

                if (userMap.TryGetValue(request.BusinessOwnerId, out var displayName))
                {
                    dto.BusinessOwnerName = displayName;
                }

                if (profileMap.TryGetValue(request.BusinessOwnerId, out var businessName))
                {
                    dto.BusinessName = businessName;
                }
            }
        }
    }
}
