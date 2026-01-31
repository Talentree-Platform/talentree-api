// Talentree.Core/Entities/Identity/UserOtp.cs

namespace Talentree.Core.Entities.Identity
{
    public enum OtpPurpose
    {
        VerifyEmail = 1,
        ResetPassword = 2
    }

    public class OtpCode
    {
        public int Id { get; set; }

        // FK
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        // OTP data
        public string Code { get; set; } = string.Empty; // 6-digit
        public OtpPurpose Purpose { get; set; }

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }

        // Computed
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsValid => !IsExpired && !IsUsed;
    }
}
