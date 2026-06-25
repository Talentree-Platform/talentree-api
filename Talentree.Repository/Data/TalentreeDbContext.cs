using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Data.Interceptors;

namespace Talentree.Repository.Data
{
    /// <summary>
    /// Main database context for Talentree e-commerce platform
    /// Handles all product, order, and customer-related data
    /// </summary>
    public class TalentreeDbContext : IdentityDbContext<AppUser>
    {
        // ===============================
        // Constructor
        // ===============================
        //interceptor
        private readonly AuditInterceptor _auditInterceptor;
        public TalentreeDbContext(DbContextOptions<TalentreeDbContext> options, AuditInterceptor auditInterceptor)
            : base(options)
        {
            _auditInterceptor = auditInterceptor;
        }

        // ===============================
        // DbSets (Database Tables)
        // ===============================

        public DbSet<Product> Products { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<BusinessOwnerProfile> BusinessOwnerProfiles { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierReview> SupplierReviews { get; set; }
        public DbSet<MaterialBasket> MaterialBaskets { get; set; }
        public DbSet<MaterialBasketItem> MaterialBasketItems { get; set; }

        public DbSet<MaterialOrder> MaterialOrders { get; set; }
        public DbSet<MaterialOrderItem> MaterialOrderItems { get; set; }

        public DbSet<BoProductionRequest> BoProductionRequests { get; set; }
        public DbSet<BoProductionRequestItem> BoProductionRequestItems { get; set; }
        public DbSet<BoProductionRequestStatusHistory> BoProductionRequestStatusHistories { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }

        public DbSet<BusinessOwnerPaymentInfo> BusinessOwnerPaymentInfos { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PayoutRequest> PayoutRequests { get; set; }




        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketMessage> TicketMessages { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ReviewPhoto> ReviewPhotos { get; set; }
     
        public DbSet<OnboardingProgress> OnboardingProgresses { get; set; }

        public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }
        public DbSet<ArticleBookmark> ArticleBookmarks { get; set; }

        // FR-AD-43: Search term analytics
        public DbSet<ContentSearchLog> ContentSearchLogs { get; set; }

        // User management and moderation logs
        public DbSet<UserActionLog> UserActionLogs { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<AutoBlockLog> AutoBlockLogs { get; set; }
        public DbSet<SecuritySettings> SecuritySettings { get; set; }

        // Customer Module Branch 2
        public DbSet<CustomerCart> CustomerCarts { get; set; }
        public DbSet<CustomerCartItem> CustomerCartItems { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<CustomerWishlist> CustomerWishlists { get; set; }
        public DbSet<CustomerWishlistItem> CustomerWishlistItems { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

        // ===============================
        // Model Configuration
        // ===============================
        /// <summary>
        /// Configures the entity models and relationships
        /// Automatically applies all IEntityTypeConfiguration implementations
        /// from the current assembly
        /// </summary>
        /// <param name="modelBuilder">The model builder instance</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<ProcessedMessage>()
                .HasKey(m => m.MessageId);

            // Fix multiple cascade paths for RefundRequest
            modelBuilder.Entity<RefundRequest>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Apply all entity configurations from this assembly
            // This will find all classes implementing IEntityTypeConfiguration<T>
            // and apply them automatically (e.g., ProductConfiguration, CategoryConfiguration)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TalentreeDbContext).Assembly);


            ApplyGlobalFilters(modelBuilder);

        }

        private void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            // Get all entity types
            var entityTypes = modelBuilder.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                // ═══════════════════════════════════════════════════════════
                // Soft Delete Filter (ISoftDelete)
                // ═══════════════════════════════════════════════════════════
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var filterExpression = Expression.Lambda(
                        Expression.Equal(property, Expression.Constant(false)),
                        parameter
                    );

                    entityType.SetQueryFilter(filterExpression);
                }


            }


        }

        public override int SaveChanges()
        {
            EnforceImmutabilityAndSuperAdminProtection();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            EnforceImmutabilityAndSuperAdminProtection();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void EnforceImmutabilityAndSuperAdminProtection()
        {
            // 1. Audit logs and login histories immutability check
            var forbiddenLogs = ChangeTracker.Entries()
                .Where(e => (e.Entity is UserActionLog || e.Entity is LoginHistory) &&
                            (e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            if (forbiddenLogs.Any())
            {
                throw new InvalidOperationException("Audit logs and login histories are immutable and cannot be updated or deleted.");
            }

            // 2. Active SuperAdmin protection checks (Prevent delete, demote, deactivate)
            var appUserEntries = ChangeTracker.Entries<AppUser>().ToList();
            var userRoleEntries = ChangeTracker.Entries<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToList();

            // Find SuperAdmin role ID
            var superAdminRole = Roles.FirstOrDefault(r => r.Name == "SuperAdmin");
            var superAdminRoleId = superAdminRole?.Id;

            // Check if any SuperAdmin is deleted, deactivated, or has their role removed
            var deletedUsers = appUserEntries.Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();
            var deactivatedUsers = appUserEntries.Where(e => e.State == EntityState.Modified && !e.Entity.IsActive && (bool)e.Property(nameof(AppUser.IsActive)).OriginalValue == true).Select(e => e.Entity).ToList();
            
            var removedSuperAdminRoles = userRoleEntries
                .Where(e => e.State == EntityState.Deleted && e.Entity.RoleId == superAdminRoleId)
                .Select(e => e.Entity.UserId)
                .ToList();

            if (deletedUsers.Any() || deactivatedUsers.Any() || removedSuperAdminRoles.Any())
            {
                // Fetch all active SuperAdmin user IDs currently in the database
                var activeSuperAdminUserIds = Users
                    .Where(u => u.IsActive)
                    .Join(UserRoles.Where(ur => ur.RoleId == superAdminRoleId),
                          u => u.Id,
                          ur => ur.UserId,
                          (u, ur) => u.Id)
                    .ToList();

                var affectedUserIds = deletedUsers.Select(u => u.Id)
                    .Concat(deactivatedUsers.Select(u => u.Id))
                    .Concat(removedSuperAdminRoles)
                    .Distinct()
                    .ToList();

                // Check remaining active SuperAdmins
                var remainingActiveSuperAdmins = activeSuperAdminUserIds.Except(affectedUserIds).ToList();
                if (!remainingActiveSuperAdmins.Any())
                {
                    throw new InvalidOperationException("Operation aborted: A system must always have at least one active SuperAdmin account. Cannot delete, deactivate, or demote the last active SuperAdmin.");
                }

                // Protect primary emergency SuperAdmin
                var primarySuperAdmin = Users.FirstOrDefault(u => u.Email == "projecttalentree@gmail.com");
                if (primarySuperAdmin != null)
                {
                    bool isPrimaryAffected = affectedUserIds.Contains(primarySuperAdmin.Id);
                    if (isPrimaryAffected)
                    {
                        throw new InvalidOperationException("Operation aborted: The primary emergency SuperAdmin account (projecttalentree@gmail.com) cannot be deleted, deactivated, or demoted.");
                    }
                }
            }
        }
    }
}
