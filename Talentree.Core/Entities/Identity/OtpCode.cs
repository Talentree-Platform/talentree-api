// Talentree.Core/Entities/Identity/UserOtp.cs

using System.ComponentModel.DataAnnotations;

namespace Talentree.Core.Entities.Identity
{
    /// <summary>
    /// Stores OTP codes for email verification and password reset
    /// </summary>
    public class UserOtp
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User ID this OTP belongs to
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Hashed OTP code (never store plain text!)
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string CodeHash { get; set; }

        /// <summary>
        /// Purpose of OTP: VerifyEmail or ResetPassword
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Purpose { get; set; } // "VerifyEmail" or "ResetPassword"

        /// <summary>
        /// When OTP expires (typically 5-10 minutes)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Number of failed attempts to use this OTP
        /// </summary>
        public int Attempts { get; set; } = 0;

        /// <summary>
        /// Maximum allowed attempts before OTP is invalidated
        /// </summary>
        public int MaxAttempts { get; set; } = 5;

        /// <summary>
        /// Whether this OTP has been used successfully
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// When OTP was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property to user
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Check if OTP is still valid
        /// </summary>
        public bool IsValid()
        {
            return !IsUsed
                   && Attempts < MaxAttempts
                   && DateTime.UtcNow < ExpiresAt;
        }
    }
}