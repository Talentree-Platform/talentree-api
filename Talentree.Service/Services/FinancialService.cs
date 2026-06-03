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

        // Admin-facing
        public async Task<Pagination<Talentree.Service.DTOs.Admin.Transactions.AdminTransactionDto>> GetAdminTransactionsAsync(
            Talentree.Service.DTOs.Admin.Transactions.AdminTransactionFilterDto filter)
        {
            var listSpec = new Talentree.Core.Specifications.TransactionSpecifications.AdminTransactionsSpecification(
                filter.BoId, filter.Type, filter.DateFrom, filter.DateTo, filter.AnomalyFlaggedOnly, filter.Search, filter.PageIndex, filter.PageSize);
                
            var countSpec = new Talentree.Core.Specifications.TransactionSpecifications.AdminTransactionCountSpecification(
                filter.BoId, filter.Type, filter.DateFrom, filter.DateTo, filter.AnomalyFlaggedOnly, filter.Search);

            var transactions = await _unitOfWork.Repository<Transaction>().GetAllWithSpecificationsAsync(listSpec);
            var total = await _unitOfWork.Repository<Transaction>().GetCountWithSpecificationsAsync(countSpec);

            var dtos = _mapper.Map<List<Talentree.Service.DTOs.Admin.Transactions.AdminTransactionDto>>(transactions);
            return new Pagination<Talentree.Service.DTOs.Admin.Transactions.AdminTransactionDto>(filter.PageIndex, filter.PageSize, total, dtos);
        }

        public async Task<Talentree.Service.DTOs.Admin.Financial.AdminFinancialReportDto> GetAdminFinancialReportAsync(
            DateTime? from, DateTime? to)
        {
            var dateFrom = from ?? DateTime.UtcNow.AddMonths(-1);
            var dateTo = to ?? DateTime.UtcNow;
            
            // This is a simplified approach fetching all in range. For huge ledgers, this would need a direct SQL query.
            var transactions = await _unitOfWork.Repository<Transaction>().GetAllAsync();
            var inRange = transactions.Where(t => t.CreatedAt >= dateFrom && t.CreatedAt <= dateTo).ToList();
            
            var report = new Talentree.Service.DTOs.Admin.Financial.AdminFinancialReportDto
            {
                PeriodFrom = dateFrom,
                PeriodTo = dateTo,
                TotalRevenue = inRange.Where(t => t.Type == TransactionType.Sale).Sum(t => t.Amount),
                TotalPayouts = inRange.Where(t => t.Type == TransactionType.Payout).Sum(t => Math.Abs(t.Amount)),
                TotalRefunds = inRange.Where(t => t.Type == TransactionType.Refund).Sum(t => Math.Abs(t.Amount)),
                TotalCommissions = inRange.Where(t => t.Type == TransactionType.Fee).Sum(t => Math.Abs(t.Amount)),
            };
            
            report.NetPlatformRevenue = report.TotalCommissions;

            var groupedByBo = inRange.Where(t => t.Type == TransactionType.Sale)
                                     .GroupBy(t => t.BusinessOwnerId);
                                     
            foreach (var group in groupedByBo.OrderByDescending(g => g.Sum(t => t.Amount)).Take(5))
            {
                // To get BO name easily, we map BusinessOwnerId in DTO if needed, or query it
                report.TopSellersByRevenue.Add(new Talentree.Service.DTOs.Admin.Financial.TopSellerDto
                {
                    BusinessOwnerId = group.Key,
                    BusinessName = group.Key, // Ideally join with AppUser, but using ID for now
                    TotalRevenue = group.Sum(t => t.Amount)
                });
            }

            var daily = inRange.Where(t => t.Type == TransactionType.Sale)
                               .GroupBy(t => t.CreatedAt.Date)
                               .Select(g => new Talentree.Service.DTOs.Admin.Financial.DailyRevenueDto
                               {
                                   Date = g.Key,
                                   Revenue = g.Sum(t => t.Amount)
                               }).OrderBy(d => d.Date).ToList();
                               
            report.DailyRevenueSeries = daily;

            return report;
        }
    }
}