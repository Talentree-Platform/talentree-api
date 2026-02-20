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
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);
            builder.ToTable("Products");

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.Tags)
                .HasMaxLength(500);

            builder.Property(p => p.Status)
                .IsRequired();

            builder.Property(p => p.RejectionReason)
                .HasMaxLength(1000);

            builder.Property(p => p.ApprovedBy)
                .HasMaxLength(450);

            // Relationships
            builder.HasOne(p => p.BusinessOwner)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BusinessOwnerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.BusinessOwnerProfileId);
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.Price);
            builder.HasIndex(p => p.Name);
        }
    }
}