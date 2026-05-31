using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.PayoutRequests;
using Talentree.Core.Specifications.Transactions;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Payout;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Manages BO payout requests and Admin payout processing.
    ///
    /// Flow:
    ///   BO submits request (Pending)
    ///   → Admin reviews → Approve or Reject
    ///   → If approved: Admin marks Complete when funds are transferred
    ///
    /// Rules:
    ///   - Only one Pending request per BO at a time
    ///   - Amount must be ≤ available balance
    ///   - Bank account identifier is encrypted before storage (AES-256 recommended)
    ///   - Displayed masked in all API responses (****1234)
    /// </summary>
    public class PayoutService : IPayoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PayoutService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ── BO actions ─────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<PayoutRequestDto> CreateRequestAsync(
            string boId, CreatePayoutRequestDto dto)
        {
            // Rule 1: Only one pending request at a time
            var pendingSpec = new PendingPayoutByBoSpecification(boId);
            var existingPending = await _unitOfWork.Repository<PayoutRequest>()
                .GetByIdWithSpecificationsAsync(pendingSpec);

            if (existingPending is not null)
                throw new InvalidOperationException(
                    "You already have a pending payout request. " +
                    "Please wait for it to be processed before submitting another.");

            // Rule 2: Amount must not exceed available balance
            var latestTxSpec = new TransactionsByBoSpecification(boId, null, 1, 1);
            var latestTx = await _unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationsAsync(latestTxSpec);
            var balance = latestTx.FirstOrDefault()?.BalanceAfter ?? 0m;

            if (dto.Amount > balance)
                throw new InvalidOperationException(
                    $"Requested amount ({dto.Amount:C}) exceeds your available balance ({balance:C}).");

            // NOTE: In production, encrypt AccountIdentifier with AES-256 before storing.
            // For MVP, we store as-is and rely on HTTPS + DB access controls.
            // TODO: inject IEncryptionService and replace line below with encrypted value.
            var request = new PayoutRequest
            {
                BusinessOwnerId = boId,
                Amount = dto.Amount,
                Currency = "EGP",
                Status = PayoutStatus.Pending,
                BankName = dto.BankName,
                AccountHolderName = dto.AccountHolderName,
                AccountIdentifierEnc = dto.AccountIdentifier, // TODO: encrypt
                RoutingSwiftCode = dto.RoutingSwiftCode
            };

            _unitOfWork.Repository<PayoutRequest>().Add(request);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PayoutRequestDto>(request);
        }

        /// <inheritdoc/>
        public async Task<Pagination<PayoutRequestDto>> GetBoHistoryAsync(
            string boId, int pageIndex, int pageSize)
        {
            var listSpec = new PayoutRequestsByBoSpecification(boId, pageIndex, pageSize);
            var countSpec = new PayoutRequestsCountByBoSpecification(boId);

            var requests = await _unitOfWork.Repository<PayoutRequest>()
                .GetAllWithSpecificationsAsync(listSpec);
            var total = await _unitOfWork.Repository<PayoutRequest>()
                .GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<PayoutRequestDto>>(requests);
            return new Pagination<PayoutRequestDto>(pageIndex, pageSize, total, dtos);
        }

        // ── Admin actions ──────────────────────────────────────

        /// <inheritdoc/>
        public async Task<Pagination<PayoutRequestDto>> GetAllAsync(
            PayoutStatus? status, int pageIndex, int pageSize)
        {
            var listSpec = new AllPayoutRequestsSpecification(status, pageIndex, pageSize);
            var countSpec = new AllPayoutRequestsCountSpecification(status);

            var requests = await _unitOfWork.Repository<PayoutRequest>()
                .GetAllWithSpecificationsAsync(listSpec);
            var total = await _unitOfWork.Repository<PayoutRequest>()
                .GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<PayoutRequestDto>>(requests);
            return new Pagination<PayoutRequestDto>(pageIndex, pageSize, total, dtos);
        }

        /// <inheritdoc/>
        public async Task<PayoutRequestDto> ApproveAsync(int requestId, string adminId)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, PayoutStatus.Pending);

            request.Status = PayoutStatus.Approved;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedBy = adminId;

            await _unitOfWork.CompleteAsync();
            // TODO: notify BO — payout request approved, processing started
            return _mapper.Map<PayoutRequestDto>(request);
        }

        /// <inheritdoc/>
        public async Task<PayoutRequestDto> RejectAsync(
            int requestId, string adminId, RejectPayoutDto dto)
        {
            var request = await LoadRequestAsync(requestId);

            if (request.Status is PayoutStatus.Completed or PayoutStatus.Rejected)
                throw new InvalidOperationException(
                    $"Cannot reject a payout request with status '{request.Status}'.");

            request.Status = PayoutStatus.Rejected;
            request.RejectionReason = dto.Reason;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedBy = adminId;

            await _unitOfWork.CompleteAsync();
            // TODO: notify BO — payout request rejected with reason
            return _mapper.Map<PayoutRequestDto>(request);
        }

        /// <inheritdoc/>
        public async Task<PayoutRequestDto> CompleteAsync(int requestId, string adminId)
        {
            var request = await LoadRequestAsync(requestId);
            AssertStatus(request, PayoutStatus.Approved);

            request.Status = PayoutStatus.Completed;
            request.ProcessedAt = DateTime.UtcNow;
            request.ProcessedBy = adminId;

            // Create a ledger entry for the payout disbursement
            // Get current balance
            var latestTxSpec = new TransactionsByBoSpecification(
                request.BusinessOwnerId, null, 1, 1);
            var latestTx = await _unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationsAsync(latestTxSpec);
            var currentBalance = latestTx.FirstOrDefault()?.BalanceAfter ?? 0m;

            var transaction = new Transaction
            {
                BusinessOwnerId = request.BusinessOwnerId,
                Type = TransactionType.Payout,
                Description = $"Payout to {request.BankName} — {request.AccountHolderName}",
                Amount = -request.Amount,    // debit — money leaving Talentree to BO's bank
                BalanceAfter = currentBalance - request.Amount,
                ReferenceId = requestId,
                ReferenceType = "PayoutRequest",
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Transaction>().Add(transaction);
            await _unitOfWork.CompleteAsync();

            // TODO: notify BO — payout completed, funds transferred
            return _mapper.Map<PayoutRequestDto>(request);
        }

        // ── Private helpers ────────────────────────────────────

        private async Task<PayoutRequest> LoadRequestAsync(int requestId)
        {
            var request = await _unitOfWork.Repository<PayoutRequest>().GetByIdAsync(requestId)
                ?? throw new KeyNotFoundException($"Payout request #{requestId} not found.");
            return request;
        }

        private static void AssertStatus(PayoutRequest request, PayoutStatus expected)
        {
            if (request.Status != expected)
                throw new InvalidOperationException(
                    $"Expected status '{expected}' but current status is '{request.Status}'.");
        }
    }
}