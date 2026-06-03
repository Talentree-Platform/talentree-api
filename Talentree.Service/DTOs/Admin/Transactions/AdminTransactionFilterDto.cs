using System;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin.Transactions
{
    public class AdminTransactionFilterDto
    {
        public string? BoId { get; set; }
        public TransactionType? Type { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool AnomalyFlaggedOnly { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
