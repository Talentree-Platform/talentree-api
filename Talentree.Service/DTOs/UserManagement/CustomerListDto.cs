using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.UserManagement
{
    public class CustomerListDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }

        public AccountStatus AccountStatus { get; set; }
        public string AccountStatusText { get; set; } = string.Empty;

        public bool IsBlocked { get; set; }
    }
}