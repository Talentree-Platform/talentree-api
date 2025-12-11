using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data
{
    /// <summary>
    /// Main database context for Talentree e-commerce platform
    /// Handles all product, order, and customer-related data
    /// </summary>
    public class TalentreeDbContext : DbContext
    {
        // ===============================
        // Constructor
        // ===============================
        /// <summary>
        /// Initializes a new instance of TalentreeDbContext
        /// </summary>
        /// <param name="options">DbContext configuration options injected by DI container</param>
        public TalentreeDbContext(DbContextOptions<TalentreeDbContext> options)
            : base(options)
        {
        }

        // ===============================
        // DbSets (Database Tables)
        // ===============================
        // TODO: Add your entities here as you create them
        // Example:
         public DbSet<Product> Products { get; set; }


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
            // Apply all entity configurations from this assembly
            // This will find all classes implementing IEntityTypeConfiguration<T>
            // and apply them automatically (e.g., ProductConfiguration, CategoryConfiguration)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TalentreeDbContext).Assembly);

        }

        
    }
}
