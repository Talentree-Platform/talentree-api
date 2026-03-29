using Microsoft.AspNetCore.Identity;
using Talentree.Core.Entities.Identity;

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
}
