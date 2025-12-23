using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Talentree.Core.Entities.Identity;
using Talentree.Repository.Identity;
using Talentree.Repository.Identity.Config;

namespace Talentree.Repository.Identity
{
    /// <summary>
    /// Database context for Identity and authentication-related data
    /// Handles user accounts, roles, claims, and related entities
    /// </summary>
    /// <remarks>
    /// Why separate from TalentreeDbContext?
    /// 
    /// Separation of Concerns:
    /// - Identity data (users, roles, auth) separate from business data (products, orders)
    /// - Independent migration history
    /// - Easier to maintain and test
    /// - Clear responsibility boundaries
    /// 
    /// Why inherit from IdentityDbContext?
    /// - Provides built-in Identity tables:
    ///   * AspNetUsers (user accounts)
    ///   * AspNetRoles (roles: Customer, BusinessOwner, Admin)
    ///   * AspNetUserRoles (user-role assignments)
    ///   * AspNetUserClaims (custom user claims)
    ///   * AspNetUserLogins (external login providers like Google, Facebook)
    ///   * AspNetUserTokens (auth tokens, refresh tokens)
    ///   * AspNetRoleClaims (role-based permissions)
    /// 
    /// Connection String:
    /// - Uses SAME connection string as TalentreeDbContext
    /// - All tables go to the same database (TalentreeDb)
    /// - But managed by different DbContext for organization
    /// </remarks>
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        /// <summary>
        /// Constructor accepting configuration options from DI container
        /// </summary>
        /// <param name="options">DbContext configuration including connection string</param>
        /// <remarks>
        /// Options are injected by ASP.NET Core DI:
        /// - Connection string from appsettings.json or User Secrets
        /// - Other EF Core configurations (logging, retry policy, etc.)
        /// </remarks>
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options)
        {
            // Base constructor sets up IdentityDbContext
            // No additional logic needed here
        }

        // ===============================
        // DbSets (Additional Tables)
        // ===============================

        /// <summary>
        /// Addresses table for user shipping/billing information
        /// </summary>
        /// <remarks>
        /// One-to-One with AppUser:
        /// - Each user can have one address
        /// - Not all users have an address (optional)
        /// - Loaded separately for performance (not always needed)
        /// </remarks>
        public DbSet<Address> Addresses { get; set; }

        /// <summary>
        /// Business owner profiles table
        /// Contains business-specific data and approval workflow
        /// </summary>
        /// <remarks>
        /// One-to-One with AppUser:
        /// - Only users with "BusinessOwner" role have this
        /// - Contains approval status, business info, tax details
        /// - Separate for security (sensitive business data)
        /// </remarks>
        public DbSet<BusinessOwnerProfile> BusinessOwnerProfiles { get; set; }

        // Note: AspNetUsers, AspNetRoles, etc. are automatically created by IdentityDbContext
        // We don't need to declare DbSet<AppUser> because base class handles it

        /// <summary>
        /// Configures entity models, relationships, and constraints
        /// Called by EF Core when building the model
        /// </summary>
        /// <param name="builder">Model builder for fluent configuration</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ===============================
            // Step 1: Apply Identity configurations
            // ===============================

            // Call base to configure all Identity tables
            // This creates: AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
            base.OnModelCreating(builder);

            // ===============================
            // Step 2: Apply custom entity configurations
            // ===============================

            // Automatically finds and applies all IEntityTypeConfiguration<T> classes
            // from this assembly (Talentree.Repository)
            // 
            // Will find:
            // - AddressConfiguration
            // - BusinessOwnerProfileConfiguration
            // 
            // Benefits:
            // - Keeps OnModelCreating clean
            // - Each entity has its own configuration file
            // - Easy to maintain and test

            ////  Apply ONLY Identity configurations explicitly
            builder.ApplyConfiguration(new AddressConfiguration());
            builder.ApplyConfiguration(new BusinessOwnerProfileConfiguration());

            ////OR

            //// Apply only configurations from Identity.Config namespace
            //var assembly = typeof(AppIdentityDbContext).Assembly;

            //var identityConfigTypes = assembly.GetTypes()
            //    .Where(t => t.Namespace == "Talentree.Repository.Identity.Config");

            //foreach (var type in identityConfigTypes)
            //{
            //    if (typeof(IEntityTypeConfiguration<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            //    {
            //        dynamic config = Activator.CreateInstance(type);
            //        builder.ApplyConfiguration(config);
            //    }
            //}


            // ===============================
            // Step 3: Customize Identity table names (Optional)
            // ===============================

            // By default, Identity creates tables with "AspNet" prefix:
            // - AspNetUsers, AspNetRoles, etc.
            // 
            // You can rename them here if needed:
            // builder.Entity<AppUser>().ToTable("Users");
            // builder.Entity<IdentityRole>().ToTable("Roles");
            // builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            // 
            // For now, we keep default names (AspNet prefix)
            // Easier to identify Identity tables vs Business tables
        }
    }
}