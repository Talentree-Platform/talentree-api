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

    // AI/Analytics fields
    public int LoginCount { get; set; } = 0;

    public float? ChurnRiskScore { get; set; }

    public DateTime? ChurnRiskUpdatedAt { get; set; } 
}
