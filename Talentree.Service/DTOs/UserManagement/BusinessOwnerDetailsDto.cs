using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.UserManagement
{
    public class BusinessOwnerDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        // Business Info
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessDescription { get; set; } = string.Empty;
        public string BusinessCategory { get; set; } = string.Empty;
        public string? BusinessAddress { get; set; }
        public string? TaxId { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? BusinessLogoUrl { get; set; }

        // Social Links
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WebsiteLink { get; set; }

        // Status
        public ApprovalStatus ApprovalStatus { get; set; }
        public string ApprovalStatusText { get; set; } = string.Empty;
        public AccountStatus AccountStatus { get; set; }
        public string AccountStatusText { get; set; } = string.Empty;

        // Approval Details
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }

        // Account Actions
        public DateTime? SuspendedAt { get; set; }
        public string? SuspendedBy { get; set; }
        public string? SuspensionReason { get; set; }

        public DateTime? BannedAt { get; set; }
        public string? BannedBy { get; set; }
        public string? BanReason { get; set; }

        public bool IsBlocked { get; set; }
        public DateTime? BlockedAt { get; set; }
        public string? BlockedBy { get; set; }
        public string? BlockReason { get; set; }

        // Activity
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int LoginAttempts { get; set; }

        // Stats
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ComplaintCount { get; set; }

        // Logs
        public List<UserActionLogDto> ActionLogs { get; set; } = new();
    }
}