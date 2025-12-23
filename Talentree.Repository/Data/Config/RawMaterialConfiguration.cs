
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// Entity configuration for RawMaterial
    /// </summary>
    public class RawMaterialConfiguration : IEntityTypeConfiguration<RawMaterial>
    {
        public void Configure(EntityTypeBuilder<RawMaterial> builder)
        {
            // Table name
            builder.ToTable("RawMaterials");

            // Primary key
            builder.HasKey(rm => rm.Id);

            // Properties
            builder.Property(rm => rm.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(rm => rm.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(rm => rm.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(rm => rm.Unit)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(rm => rm.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(rm => rm.PictureUrl)
                .HasMaxLength(500);

            builder.Property(rm => rm.MinimumOrderQuantity)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(rm => rm.StockQuantity)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(rm => rm.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);


            // Relationships configured in SupplierConfiguration

            // Indexes
            builder.HasIndex(rm => rm.SupplierId);
            builder.HasIndex(rm => rm.Category);
            builder.HasIndex(rm => rm.IsAvailable);
            builder.HasIndex(rm => rm.Price);
        }
    }
}