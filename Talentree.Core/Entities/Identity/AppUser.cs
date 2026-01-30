// Talentree.Core/Entities/Identity/AppUser.cs

using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Talentree.Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        // ⭐ NEW - Track email verification separately from Identity's EmailConfirmed
        // This gives us more control over the verification flow
        public bool IsEmailVerified { get; set; } = false;

        // Navigation properties
        public Address Address { get; set; }
        public BusinessOwnerProfile? BusinessOwnerProfile { get; set; }
    }
}