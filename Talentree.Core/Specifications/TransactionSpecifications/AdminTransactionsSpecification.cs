using System;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.TransactionSpecifications
{
    public class AdminTransactionsSpecification : BaseSpecifications<Transaction>
    {
        public AdminTransactionsSpecification(string? boId, TransactionType? type, DateTime? dateFrom, DateTime? dateTo, bool anomalyFlaggedOnly, string? search, int pageIndex, int pageSize)
            : base(x => 
                (string.IsNullOrEmpty(boId) || x.BusinessOwnerId == boId) &&
                (!type.HasValue || x.Type == type) &&
                (!dateFrom.HasValue || x.CreatedAt >= dateFrom) &&
                (!dateTo.HasValue || x.CreatedAt <= dateTo) &&
                (!anomalyFlaggedOnly || x.AnomalyFlag == true) &&
                (string.IsNullOrEmpty(search) || x.Description.Contains(search))
            )
        {
            AddOrderByDescending(x => x.CreatedAt);
            ApplyPagination(pageIndex, pageSize);
        }
    }
}
