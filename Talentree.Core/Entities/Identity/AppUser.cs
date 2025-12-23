using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Numerics;
using Talentree.Core.Entities.Identity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Talentree.Core.Entities.Identity
{
    /// <summary>
    /// Application user entity extending IdentityUser
    /// Represents a user account in the Talentree platform
    /// </summary>
    /// <remarks>
    /// IdentityUser provides built-in properties like:
    /// - Email, UserName, PasswordHash (authentication)
    /// - PhoneNumber, EmailConfirmed (verification)
    /// - AccessFailedCount, LockoutEnd (security)
    /// 
    /// We extend it with custom properties for business needs
    /// </remarks>
    public class AppUser : IdentityUser
    {
        /// <summary>
        /// User's display name shown in the UI
        /// </summary>
        /// <remarks>
        /// Why separate from UserName?
        /// - UserName is for login (unique, no spaces)
        /// - DisplayName is for display (can have spaces, special chars)
        /// Example: UserName = "john.doe" but DisplayName = "John Doe"
        /// </remarks>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the user account was created (UTC)
        /// </summary>
        /// <remarks>
        /// Useful for:
        /// - Audit trail (when did user join?)
        /// - Analytics (user growth over time)
        /// - Sorting users by registration date
        /// </remarks>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of the user's last successful login (UTC)
        /// Null if user never logged in
        /// </summary>
        /// <remarks>
        /// Useful for:
        /// - Identifying inactive users
        /// - Security monitoring (unusual login patterns)
        /// - User engagement metrics
        /// </remarks>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Indicates if the user account is active
        /// Admins can deactivate accounts without deleting them
        /// </summary>
        /// <remarks>
        /// Why not just delete?
        /// - Soft deactivation (can be reactivated)
        /// - Preserve historical data (orders, reviews)
        /// - Compliance (some regulations require data retention)
        /// </remarks>
        public bool IsActive { get; set; } = true;

        // ===============================
        // Navigation Properties
        // ===============================

        /// <summary>
        /// User's shipping/billing address (optional)
        /// One-to-One relationship
        /// </summary>
        /// <remarks>
        /// Why nullable?
        /// - Not all users provide address immediately
        /// - Business owners might not need it
        /// - Can be added later when placing first order
        /// </remarks>
        public Address? Address { get; set; }

        /// <summary>
        /// Business owner profile (only for users with BusinessOwner role)
        /// One-to-One relationship
        /// </summary>
        /// <remarks>
        /// Why separate entity?
        /// - Not all users are business owners
        /// - Business-specific data (tax ID, approval status)
        /// - Keeps AppUser clean (Single Responsibility Principle)
        /// </remarks>
        public BusinessOwnerProfile? BusinessOwnerProfile { get; set; }
    }
}