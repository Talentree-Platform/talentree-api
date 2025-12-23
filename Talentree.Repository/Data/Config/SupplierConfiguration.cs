
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// Entity configuration for Supplier
    /// </summary>
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            // Table name
            builder.ToTable("Suppliers");

            // Primary key
            builder.HasKey(s => s.Id);

            // Properties
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.TaxId)
                .HasMaxLength(50);

            builder.Property(s => s.ContactPerson)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.CreatedBy)
                .IsRequired()
                .HasMaxLength(450); // Same as AspNetUsers.Id

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
          

            // Relationships
            builder.HasMany(s => s.RawMaterials)
                .WithOne(rm => rm.Supplier)
                .HasForeignKey(rm => rm.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(s => s.Email);
            builder.HasIndex(s => s.IsActive);
        }
    }
}