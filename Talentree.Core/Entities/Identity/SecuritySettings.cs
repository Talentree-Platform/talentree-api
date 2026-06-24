using System;

namespace Talentree.Core.Entities.Identity
{
    public class SecuritySettings : BaseEntity
    {
        public int PasswordRequiredLength { get; set; } = 8;
        public bool PasswordRequireDigit { get; set; } = true;
        public bool PasswordRequireLowercase { get; set; } = true;
        public bool PasswordRequireUppercase { get; set; } = true;
        public bool PasswordRequireNonAlphanumeric { get; set; } = true;

        public int SessionTimeoutInMinutes { get; set; } = 15;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int LockoutDurationInMinutes { get; set; } = 15;

        public bool RequireTwoFactorForAdmins { get; set; } = false;

        public string? IpWhitelist { get; set; } // Comma-separated allowed IP addresses

        public TimeSpan? AllowedLoginStartTime { get; set; }
        public TimeSpan? AllowedLoginEndTime { get; set; }
    }
}
