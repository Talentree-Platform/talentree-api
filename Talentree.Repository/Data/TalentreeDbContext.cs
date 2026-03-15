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
        public DbSet<MaterialBasket> MaterialBaskets { get; set; }
        public DbSet<MaterialBasketItem> MaterialBasketItems { get; set; }

        public DbSet<MaterialOrder> MaterialOrders { get; set; }
        public DbSet<MaterialOrderItem> MaterialOrderItems { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationPreference> NotificationPreferences { get; set; }

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
    }
}
