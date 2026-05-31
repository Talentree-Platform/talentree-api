using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.PayoutRequests;
using Talentree.Core.Specifications.Transactions;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Transaction;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Provides financial dashboard data and transaction history for Business Owners.
    /// All data is read from the Transactions ledger — this service never writes.
    /// The ledger is written exclusively by PaymentService when Stripe confirms payments.
    /// </summary>
    public class FinancialService : IFinancialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinancialService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<FinancialSummaryDto> GetSummaryAsync(
            string boId, DateTime from, DateTime to)
        {
            // Load all transactions in the date range for this BO
            var rangeSpec = new TransactionsByBoAndDateRangeSpecification(boId, from, to);
            var inRange = await _unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationsAsync(rangeSpec);

            // Total Revenue — Sale transactions (Phase 2, will be 0 until Customer module)
            var totalRevenue = inRange
                .Where(t => t.Type == TransactionType.Sale)
                .Sum(t => t.Amount);

            // Total Expenses — material purchases + production requests (always positive here)
            var totalExpenses = inRange
                .Where(t => t.Type == TransactionType.MaterialPurchase
                         || t.Type == TransactionType.ProductionRequest)
                .Sum(t => Math.Abs(t.Amount)); // amounts are stored as negatives, show as positive

            // Available balance — latest BalanceAfter across ALL time (not just the date range)
            var latestSpec = new TransactionsByBoSpecification(boId, null, 1, 1);
            var latest = await _unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationsAsync(latestSpec);
            var availableBalance = latest.FirstOrDefault()?.BalanceAfter ?? 0m;

            // Pending payouts — sum of all Pending payout requests
            var pendingSpec = new PendingPayoutByBoSpecification(boId);
            var pendingPayouts = (await _unitOfWork.Repository<PayoutRequest>()
                .GetAllWithSpecificationsAsync(pendingSpec))
                .Sum(p => p.Amount);

            return new FinancialSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                AvailableBalance = availableBalance,
                PendingPayouts = pendingPayouts,
                PeriodFrom = from,
                PeriodTo = to
            };
        }

        /// <inheritdoc/>
        public async Task<Pagination<TransactionDto>> GetTransactionHistoryAsync(
            string boId, TransactionType? type, int pageIndex, int pageSize)
        {
            var listSpec = new TransactionsByBoSpecification(boId, type, pageIndex, pageSize);
            var countSpec = new TransactionsCountByBoSpecification(boId, type);

            var transactions = await _unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationsAsync(listSpec);
            var total = await _unitOfWork.Repository<Transaction>()
                .GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<TransactionDto>>(transactions);
            return new Pagination<TransactionDto>(pageIndex, pageSize, total, dtos);
        }
    }
}