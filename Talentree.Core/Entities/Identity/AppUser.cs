using Microsoft.AspNetCore.Identity;
using Talentree.Core.Entities.Identity;

public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;

    // Navigation
    public Address? Address { get; set; }
    public BusinessOwnerProfile? BusinessOwnerProfile { get; set; }
}
