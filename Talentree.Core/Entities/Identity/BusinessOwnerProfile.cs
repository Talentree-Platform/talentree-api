using Microsoft.Win32;
using Talentree.Core.Entities.Identity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Talentree.Core.Entities.Identity
{
    /// <summary>
    /// Business owner profile containing business-specific information
    /// Only users with BusinessOwner role have this profile
    /// </summary>
    /// <remarks>
    /// Why separate from AppUser?
    /// 
    /// Separation of Concerns:
    /// - Not all users are business owners
    /// - Business-specific data doesn't clutter AppUser
    /// - Different validation rules for business data
    /// - Can query business owners separately
    /// 
    /// Business Logic:
    /// - Business owners must be approved by admin
    /// - Can have pending/approved/rejected status
    /// - Contains business legal information
    /// </remarks>
    public class BusinessOwnerProfile : BaseEntity
    {
        // ===============================
        // User Relationship
        // ===============================

        /// <summary>
        /// Foreign key to the user who owns this business
        /// One-to-One: Each user can have only one business
        /// </summary>
        public string AppUserId { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the user
        /// </summary>
        public AppUser AppUser { get; set; } = null!;

        // ===============================
        // Business Information
        // ===============================

        /// <summary>
        /// Legal business name
        /// Example: "Ahmed's Handicrafts", "Sara's Bakery"
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Public business profile
        /// - Search and discovery
        /// - Legal documentation
        /// </remarks>
        public string BusinessName { get; set; } = string.Empty;

        /// <summary>
        /// Description of what the business offers
        /// Shown on business profile page
        /// </summary>
        /// <remarks>
        /// Help customers understand:
        /// - What products you sell
        /// - Your unique selling proposition
        /// - Business story/background
        /// 
        /// Example:
        /// "Handcrafted candles made with natural ingredients.
        ///  Family business since 2020. Ships nationwide."
        /// </remarks>
        public string BusinessDescription { get; set; } = string.Empty;

        /// <summary>
        /// Business category for classification
        /// Example: "Handicrafts", "Food & Beverage", "Fashion"
        /// </summary>
        /// <remarks>
        /// Used for:
        /// - Categorizing businesses in marketplace
        /// - Filtering and search
        /// - Analytics and reporting
        /// 
        /// Consider:
        /// - Predefined list of categories (dropdown)
        /// - Or separate Category entity with FK
        /// </remarks>
        public string BusinessCategory { get; set; } = string.Empty;

        /// <summary>
        /// Tax identification number for legal compliance
        /// Required for invoicing and tax purposes
        /// </summary>
        /// <remarks>
        /// Why required?
        /// - Legal requirement for business transactions
        /// - Platform needs for tax reporting
        /// - Validates business legitimacy
        /// 
        /// Security:
        /// - Store encrypted in production
        /// - Access restricted to admin only
        /// - Never expose in API responses
        /// </remarks>
        public string? TaxId { get; set; } = string.Empty;

        // ===============================
        // Approval Workflow
        // ===============================

        /// <summary>
        /// Current approval status of the business registration
        /// Controls whether business can sell on platform
        /// </summary>
        /// <remarks>
        /// Workflow:
        /// 1. User registers → Status = Pending
        /// 2. Admin reviews → Status = Approved/Rejected
        /// 3. If no admin action in 12 hours → Status = AutoApproved
        /// 
        /// Business Logic:
        /// - Pending/Rejected: Cannot sell products
        /// - Approved/AutoApproved: Can sell products
        /// </remarks>
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        /// <summary>
        /// Timestamp when admin approved the business (UTC)
        /// Null if still pending or rejected
        /// </summary>
        /// <remarks>
        /// Audit trail:
        /// - Proves when business was verified
        /// - Legal compliance (know when business started)
        /// - Analytics (approval process metrics)
        /// </remarks>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Admin user ID who approved/rejected the business
        /// For accountability and audit
        /// </summary>
        /// <remarks>
        /// Why track approver?
        /// - Accountability (who made the decision)
        /// - Contact for questions about approval
        /// - Detect approval patterns (admin performance)
        /// </remarks>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Reason for rejection (if status is Rejected)
        /// Helps business owner understand why and improve
        /// </summary>
        /// <remarks>
        /// Examples:
        /// - "Tax ID is invalid"
        /// - "Business description violates content policy"
        /// - "Duplicate business registration"
        /// 
        /// User Experience:
        /// - Clear feedback for rejected users
        /// - Guidance on how to fix issues
        /// - Opportunity to reapply
        /// </remarks>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Deadline for automatic approval (UTC)
        /// If admin doesn't act by this time, auto-approve
        /// </summary>
        /// <remarks>
        /// Auto-approval logic:
        /// - Set to CreatedAt + 12 hours when business registers
        /// - Background job checks every hour
        /// - If DateTime.UtcNow > AutoApprovalDeadline AND Status == Pending
        ///   → Change Status to AutoApproved
        /// 
        /// Why auto-approve?
        /// - Prevent admin bottleneck
        /// - Improve user experience (faster onboarding)
        /// - Admin can still review and revoke later
        /// </remarks>
        public DateTime? AutoApprovalDeadline { get; set; }
    }

    /// <summary>
    /// Approval status for business owner registration
    /// </summary>
    public enum ApprovalStatus
    {
        /// <summary>
        /// Awaiting admin review (initial state)
        /// Business cannot sell yet
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Manually approved by admin
        /// Business can sell products
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Rejected by admin with reason
        /// Business cannot sell (can reapply)
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// Automatically approved after deadline
        /// Business can sell products
        /// Admin can still review and revoke
        /// </summary>
        AutoApproved = 4
    }
}
