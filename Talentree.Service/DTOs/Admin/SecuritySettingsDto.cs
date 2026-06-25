using System;

namespace Talentree.Service.DTOs.Admin
{
    public class SecuritySettingsDto
    {
        public int Id { get; set; }
        public int PasswordRequiredLength { get; set; }
        public bool PasswordRequireDigit { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireNonAlphanumeric { get; set; }

        public int SessionTimeoutInMinutes { get; set; }
        public int MaxFailedAccessAttempts { get; set; }
        public int LockoutDurationInMinutes { get; set; }

        public bool RequireTwoFactorForAdmins { get; set; }

        public string? IpWhitelist { get; set; }

        public TimeSpan? AllowedLoginStartTime { get; set; }
        public TimeSpan? AllowedLoginEndTime { get; set; }
    }
}
