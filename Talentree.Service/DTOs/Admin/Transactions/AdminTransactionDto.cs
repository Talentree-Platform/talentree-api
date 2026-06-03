using Talentree.Service.DTOs.Transaction;

namespace Talentree.Service.DTOs.Admin.Transactions
{
    public class AdminTransactionDto : TransactionDto
    {
        public string BusinessOwnerName { get; set; } = null!;
        public string BusinessOwnerEmail { get; set; } = null!;
        public bool AnomalyFlag { get; set; }
        public float? AnomalyScore { get; set; }
    }
}
