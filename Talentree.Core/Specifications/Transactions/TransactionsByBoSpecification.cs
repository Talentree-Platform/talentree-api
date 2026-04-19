using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.Transactions
{
    /// <summary>
    /// Returns paginated transactions for a BO, newest first.
    /// Optionally filtered by transaction type.
    /// </summary>
    public class TransactionsByBoSpecification : BaseSpecifications<Transaction>
    {
        public TransactionsByBoSpecification(
            string businessOwnerId,
            TransactionType? type,
            int pageIndex,
            int pageSize)
            : base(t => t.BusinessOwnerId == businessOwnerId
                     && (type == null || t.Type == type))
        {
            AddOrderByDescending(t => t.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }

    /// <summary>
    /// Counts transactions for a BO, optionally filtered by type.
    /// Paired with TransactionsByBoSpecification for pagination totals.
    /// </summary>
    public class TransactionsCountByBoSpecification : BaseSpecifications<Transaction>
    {
        public TransactionsCountByBoSpecification(string businessOwnerId, TransactionType? type)
            : base(t => t.BusinessOwnerId == businessOwnerId
                     && (type == null || t.Type == type))
        {
        }
    }

    /// <summary>
    /// Returns all transactions for a BO within a date range.
    /// Used for financial dashboard summary cards and charts.
    /// </summary>
    public class TransactionsByBoAndDateRangeSpecification : BaseSpecifications<Transaction>
    {
        public TransactionsByBoAndDateRangeSpecification(
            string businessOwnerId,
            DateTime from,
            DateTime to)
            : base(t => t.BusinessOwnerId == businessOwnerId
                     && t.CreatedAt >= from
                     && t.CreatedAt <= to)
        {
            AddOrderBy(t => t.CreatedAt);
        }
    }
}