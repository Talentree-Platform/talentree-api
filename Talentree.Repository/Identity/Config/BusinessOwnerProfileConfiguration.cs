using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Identity.Config;

namespace Talentree.Repository.Identity.Config
{
    /// <summary>
    /// Entity Framework configuration for BusinessOwnerProfile entity
    /// Configures business-specific data structure and approval workflow
    /// </summary>
    public class BusinessOwnerProfileConfiguration : IEntityTypeConfiguration<BusinessOwnerProfile>
    {
        public void Configure(EntityTypeBuilder<BusinessOwnerProfile> builder)
        {
            // ===============================
            // Table Name
            // ===============================

            builder.ToTable("BusinessOwnerProfiles");

            // ===============================
            // Primary Key
            // ===============================

            builder.HasKey(b => b.Id);

            // ===============================
            // Business Information Fields
            // ===============================

            /// <summary>
            /// BusinessName: Required (core business identity)
            /// MaxLength: 200 characters
            /// Example: "Ahmed's Handicrafts & Home Decor"
            /// </summary>
            builder.Property(b => b.BusinessName)
                .IsRequired()
                .HasMaxLength(200);

            /// <summary>
            /// BusinessDescription: Required
            /// MaxLength: 1000 characters (allows detailed description)
            /// 
            /// Why 1000?
            /// - Enough for meaningful description
            /// - Not too long (keeps UI manageable)
            /// - Average: 150-200 words
            /// </summary>
            builder.Property(b => b.BusinessDescription)
                .IsRequired()
                .HasMaxLength(1000);

            /// <summary>
            /// BusinessCategory: Required for classification
            /// MaxLength: 100 characters
            /// Example: "Handicrafts", "Food & Beverage"
            /// 
            /// Future Enhancement:
            /// - Can be replaced with FK to separate Category table
            /// - For now, simple string is sufficient
            /// </summary>
            builder.Property(b => b.BusinessCategory)
                .IsRequired()
                .HasMaxLength(100);

            /// <summary>
            /// TaxId: Optional (made optional as per your decision)
            /// MaxLength: 50 characters (handles various tax ID formats)
            /// 
            /// Security Note:
            /// - Consider encrypting in production
            /// - Never expose in API responses
            /// - Access restricted to admin only
            /// 
            /// Why optional?
            /// - Small businesses might not have tax ID initially
            /// - Can be added later during verification
            /// - Some countries have different requirements
            /// </summary>
            builder.Property(b => b.TaxId)
                .IsRequired(false)              // Optional (nullable)
                .HasMaxLength(50);

            // ===============================
            // Approval Workflow Fields
            // ===============================

            /// <summary>
            /// Status: Required (tracks approval state)
            /// Default: Pending (when business registers)
            /// 
            /// Enum values:
            /// 1 = Pending, 2 = Approved, 3 = Rejected, 4 = AutoApproved
            /// </summary>
            builder.Property(b => b.Status)
                .IsRequired()
                .HasDefaultValue(ApprovalStatus.Pending);  // Default to Pending on insert

            /// <summary>
            /// ApprovedAt: Optional (null until approved)
            /// Timestamp when admin approved the business
            /// </summary>
            builder.Property(b => b.ApprovedAt)
                .IsRequired(false);             // Nullable

            /// <summary>
            /// ApprovedBy: Optional (null until approved)
            /// MaxLength: 450 (matches IdentityUser.Id length)
            /// 
            /// Why 450?
            /// - IdentityUser uses string (nvarchar(450)) as primary key
            /// - Must match to create proper FK later if needed
            /// </summary>
            builder.Property(b => b.ApprovedBy)
                .IsRequired(false)
                .HasMaxLength(450);

            /// <summary>
            /// RejectionReason: Optional (only for rejected businesses)
            /// MaxLength: 500 characters (enough for detailed reason)
            /// 
            /// Examples:
            /// "Invalid tax ID format"
            /// "Business description violates content policy"
            /// </summary>
            builder.Property(b => b.RejectionReason)
                .IsRequired(false)
                .HasMaxLength(500);

            /// <summary>
            /// AutoApprovalDeadline: Optional
            /// Set to CreatedAt + 12 hours when business registers
            /// 
            /// Logic:
            /// - Background job checks this periodically
            /// - If DateTime.UtcNow > AutoApprovalDeadline && Status == Pending
            ///   → Change Status to AutoApproved
            /// </summary>
            builder.Property(b => b.AutoApprovalDeadline)
                .IsRequired(false);

            // ===============================
            // Foreign Key & Relationship
            // ===============================

            /// <summary>
            /// AppUserId: Required (every business belongs to a user)
            /// </summary>
            builder.Property(b => b.AppUserId)
                .IsRequired();

            /// <summary>
            /// One-to-One relationship: AppUser → BusinessOwnerProfile
            /// 
            /// Configuration:
            /// - One user can have one business profile
            /// - Not all users have business profile (only BusinessOwner role)
            /// - OnDelete: Cascade (if user deleted, profile deleted too)
            /// 
            /// Why Cascade?
            /// - Business profile has no meaning without user
            /// - Prevents orphaned business records
            /// - Maintains data integrity
            /// </summary>
            builder.HasOne(b => b.AppUser)                          // Profile has one User
                .WithOne(u => u.BusinessOwnerProfile)               // User has one Profile
                .HasForeignKey<BusinessOwnerProfile>(b => b.AppUserId)  // FK
                .OnDelete(DeleteBehavior.Cascade);                  // Cascade delete

            // ===============================
            // Indexes for Performance
            // ===============================

            // Index on AppUserId for faster user-to-business lookups
            builder.HasIndex(b => b.AppUserId)
                .HasDatabaseName("IX_BusinessOwnerProfiles_AppUserId");

            // Index on Status for admin dashboard queries
            // Admins frequently query: "Get all pending businesses"
            builder.HasIndex(b => b.Status)
                .HasDatabaseName("IX_BusinessOwnerProfiles_Status");

            // Composite index on Status + AutoApprovalDeadline
            // For background job: "Get pending businesses past deadline"
            builder.HasIndex(b => new { b.Status, b.AutoApprovalDeadline })
                .HasDatabaseName("IX_BusinessOwnerProfiles_Status_AutoApprovalDeadline");

            // Index on BusinessCategory for filtering
            // Users browse: "Show me all Food & Beverage businesses"
            builder.HasIndex(b => b.BusinessCategory)
                .HasDatabaseName("IX_BusinessOwnerProfiles_BusinessCategory");
        }
    }
}




