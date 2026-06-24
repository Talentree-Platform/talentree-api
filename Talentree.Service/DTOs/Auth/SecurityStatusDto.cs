using System;

namespace Talentree.Service.DTOs.Auth
{
    public class SecurityStatusDto
    {
        public bool IsTwoFactorEnabled { get; set; }
        public bool MustChangePassword { get; set; }
        public int FailedAttempts { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLoginIp { get; set; }
        public int ActiveSessionCount { get; set; }
    }
}
