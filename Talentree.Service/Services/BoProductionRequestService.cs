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
    /// Handles the Business Owner side of production service requests.
    /// Covers: submit, list, detail, confirm quote, and cancel.
    /// </summary>
    public class BoProductionRequestService : IBoProductionRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Statuses from which the BO is allowed to cancel their own request.
        /// Once Talentree has started production the BO can no longer cancel.
        /// </summary>
        private static readonly BoProductionRequestStatus[] _cancellableStatuses =
        [
            BoProductionRequestStatus.Submitted,
            BoProductionRequestStatus.Quoted
        ];

        public BoProductionRequestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> SubmitRequestAsync(
            string businessOwnerId, SubmitProductionRequestDto dto)
        {
            // Validate any referenced raw material IDs
            foreach (var item in dto.Items.Where(i => i.PreferredRawMaterialId.HasValue))
            {
                var material = await _unitOfWork.Repository<RawMaterial>()
                    .GetByIdAsync(item.PreferredRawMaterialId!.Value);

                if (material is null || material.IsDeleted)
                    throw new InvalidOperationException(
                        $"Raw material #{item.PreferredRawMaterialId} not found.");
            }

            var request = new BoProductionRequest
            {
                BusinessOwnerId = businessOwnerId,
                Title = dto.Title,
                Notes = dto.Notes,
                Status = BoProductionRequestStatus.Submitted,
                Items = dto.Items.Select(i => new BoProductionRequestItem
                {
                    ProductType = i.ProductType,
                    Quantity = i.Quantity,
                    PreferredRawMaterialId = i.PreferredRawMaterialId,
                    Specifications = i.Specifications
                }).ToList()
            };

            request.StatusHistory.Add(new BoProductionRequestStatusHistory
            {
                Status = BoProductionRequestStatus.Submitted,
                ChangedByUserId = businessOwnerId,
                Notes = "Request submitted by business owner."
            });

            _unitOfWork.Repository<BoProductionRequest>().Add(request);
            await _unitOfWork.CompleteAsync();

            return await LoadBoDetailAsync(request.Id, businessOwnerId);
        }

        /// <inheritdoc/>
        public async Task<Pagination<ProductionRequestSummaryDto>> GetMyRequestsAsync(
            string businessOwnerId, int pageIndex, int pageSize)
        {
            var listSpec = new ProductionRequestsByBoSpecification(businessOwnerId, pageIndex, pageSize);
            var countSpec = new ProductionRequestsCountByBoSpecification(businessOwnerId);

            var requests = await _unitOfWork.Repository<BoProductionRequest>()
                .GetAllWithSpecificationsAsync(listSpec);
            var total = await _unitOfWork.Repository<BoProductionRequest>()
                .GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<ProductionRequestSummaryDto>>(requests);
            return new Pagination<ProductionRequestSummaryDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> GetRequestByIdAsync(
            string businessOwnerId, int requestId)
            => await LoadBoDetailAsync(requestId, businessOwnerId);

        /// <inheritdoc/>
        public async Task<ProductionRequestDetailDto> CancelRequestAsync(
            string businessOwnerId, int requestId)
        {
            var request = await GetOwnedRequestAsync(businessOwnerId, requestId);

            if (!_cancellableStatuses.Contains(request.Status))
                throw new InvalidOperationException(
                    $"Cannot cancel a request with status '{request.Status}'. " +
                    $"Cancellation is only allowed before Talentree starts production.");

            request.Status = BoProductionRequestStatus.Cancelled;
            request.StatusHistory.Add(new BoProductionRequestStatusHistory
            {
                Status = BoProductionRequestStatus.Cancelled,
                ChangedByUserId = businessOwnerId,
                Notes = "Cancelled by business owner."
            });

            await _unitOfWork.CompleteAsync();
            return await LoadBoDetailAsync(requestId, businessOwnerId);
        }

        
        // ── Private helpers ───────────────────────────────────────

        /// <summary>
        /// Loads a production request with all includes scoped to the given BO.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        private async Task<ProductionRequestDetailDto> LoadBoDetailAsync(
            int requestId, string businessOwnerId)
        {
            var spec = new ProductionRequestByIdAndBoSpecification(requestId, businessOwnerId);
            var request = await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Production request #{requestId} not found.");

            return _mapper.Map<ProductionRequestDetailDto>(request);
        }

        /// <summary>
        /// Loads a production request for mutation (with StatusHistory) scoped to the given BO.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        private async Task<BoProductionRequest> GetOwnedRequestAsync(
            string businessOwnerId, int requestId)
        {
            var spec = new ProductionRequestByIdAndBoSpecification(requestId, businessOwnerId);
            return await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Production request #{requestId} not found.");
        }
    }
}