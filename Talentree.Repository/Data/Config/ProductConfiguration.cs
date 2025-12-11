using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// Entity Framework configuration for Product entity
    /// Defines database schema, constraints, indexes, and relationships using Fluent API
    /// </summary>
    /// <remarks>
    /// Using Fluent API (instead of Data Annotations) for:
    /// - Better separation of concerns (keeps domain entities clean)
    /// - More powerful configuration options
    /// - Centralized configuration management
    /// </remarks>
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // ===============================
            // Table Configuration
            // ===============================

            // Specify table name explicitly (optional, but good practice)
            builder.ToTable("Products");

            // ===============================
            // Primary Key
            // ===============================

            // Id is already configured as primary key by convention (property named "Id")
            // But we can be explicit for clarity:
            builder.HasKey(p => p.Id);

            // ===============================
            // Properties Configuration
            // ===============================

            // Name property
            builder.Property(p => p.Name)
                .IsRequired()                      // NOT NULL constraint
                .HasMaxLength(200);                // VARCHAR(200)

            // Description property
            builder.Property(p => p.Description)
                .IsRequired()                      // NOT NULL constraint
                .HasMaxLength(1000);               // VARCHAR(1000)

            // Price property - VERY IMPORTANT for money!
            builder.Property(p => p.Price)
                .IsRequired()                      // NOT NULL constraint
                .HasColumnType("decimal(18,2)");   // DECIMAL(18,2) - prevents rounding errors

            // Why decimal(18,2)?
            // - 18 total digits
            // - 2 digits after decimal point
            // - Max value: 9,999,999,999,999,999.99
            // - Prevents floating-point precision issues with money

            // PictureUrl property
            builder.Property(p => p.PictureUrl)
                .IsRequired()                      // NOT NULL constraint
                .HasMaxLength(500);                // VARCHAR(500) - enough for URLs

            // ===============================
            // Indexes (for Query Performance)
            // ===============================

            //// Index on Name for search/filter operations
            //// When users search for products by name, this makes it FAST
            //builder.HasIndex(p => p.Name)
            //    .HasDatabaseName("IX_Products_Name");

            // Future: Add more indexes as needed
            // builder.HasIndex(p => p.Price).HasDatabaseName("IX_Products_Price");

            // ===============================
            // Relationships (Future)
            // ===============================

            // Will be configured when we add ProductType and ProductBrand relationships

            // Example (commented for now):
            // builder.HasOne(p => p.ProductType)
            //     .WithMany(pt => pt.Products)
            //     .HasForeignKey(p => p.ProductTypeId)
            //     .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion
        }
    }
}