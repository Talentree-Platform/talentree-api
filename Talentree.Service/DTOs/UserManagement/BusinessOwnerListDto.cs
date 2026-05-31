using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.UserManagement
{
    public class BusinessOwnerListDto
    {
        public string Id { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalStatusText { get; set; } = string.Empty;

        public AccountStatus AccountStatus { get; set; }
        public string AccountStatusText { get; set; } = string.Empty;

        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public bool IsSuspended { get; set; }
        public bool IsBanned { get; set; }
        public bool IsBlocked { get; set; }
    }
}