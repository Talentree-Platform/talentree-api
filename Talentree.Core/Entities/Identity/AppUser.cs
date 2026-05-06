using Microsoft.AspNetCore.Identity;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public Address? Address { get; set; }
    public BusinessOwnerProfile? BusinessOwnerProfile { get; set; }

    public BusinessOwnerPaymentInfo? PaymentInfo { get; set; }
    public UserPreferences? Preferences { get; set; }
    public ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();
    // AI/Analytics fields
    public int LoginCount { get; set; } = 0;

    public float? ChurnRiskScore { get; set; }

    public DateTime? ChurnRiskUpdatedAt { get; set; }

    // User Management Fields
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendedBy { get; set; }
    public string? SuspensionReason { get; set; }

    public DateTime? BannedAt { get; set; }
    public string? BannedBy { get; set; }
    public string? BanReason { get; set; }

    public int LoginAttempts { get; set; } = 0;

    public bool IsBlocked { get; set; } = false;
    public DateTime? BlockedAt { get; set; }
    public string? BlockedBy { get; set; }
    public string? BlockReason { get; set; }

    // ProfilePhotoUrl
     
    public string? ProfilePhotoUrl { get; set; }


}
