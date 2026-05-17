using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.UserManagement
{
    public class CustomerDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoUrl { get; set; }

        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public AccountStatus AccountStatus { get; set; }
        public string AccountStatusText { get; set; } = string.Empty;

        public bool IsBlocked { get; set; }
        public DateTime? BlockedAt { get; set; }
        public string? BlockedBy { get; set; }
        public string? BlockReason { get; set; }

        // Stats
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalReviews { get; set; }

        // Logs
        public List<UserActionLogDto> ActionLogs { get; set; } = new();
    }
}