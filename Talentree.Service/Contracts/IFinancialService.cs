using Talentree.Core.Enums;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Transaction;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Provides financial dashboard data and transaction history for Business Owners.
    /// Reads from the Transactions ledger — never writes directly.
    /// </summary>
    public interface IFinancialService
    {
        /// <summary>
        /// Returns summary cards for the dashboard:
        /// Total Revenue, Total Expenses, Net Profit, Available Balance, Pending Payouts.
        /// </summary>
        Task<FinancialSummaryDto> GetSummaryAsync(string boId, DateTime from, DateTime to);

        /// <summary>
        /// Returns paginated transaction history for the BO, newest first.
        /// Optionally filtered by transaction type.
        /// </summary>
        Task<Pagination<TransactionDto>> GetTransactionHistoryAsync(
            string boId, TransactionType? type, int pageIndex, int pageSize);
    }
}