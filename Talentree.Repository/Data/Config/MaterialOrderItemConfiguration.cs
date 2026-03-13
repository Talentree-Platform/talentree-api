using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// EF Core configuration for <see cref="MaterialOrderItem"/>.
    /// Ignores the computed <c>LineTotal</c> property.
    /// Uses Restrict on RawMaterial FK to preserve order history if a material is deleted.
    /// </summary>
    public class MaterialOrderItemConfiguration : IEntityTypeConfiguration<MaterialOrderItem>
    {
        public void Configure(EntityTypeBuilder<MaterialOrderItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.UnitPriceAtPurchase)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // LineTotal is computed — do not map to a column
            builder.Ignore(i => i.LineTotal);

            builder.HasOne(i => i.RawMaterial)
                .WithMany()
                .HasForeignKey(i => i.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}