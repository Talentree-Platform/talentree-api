using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for <see cref="BoProductionRequestItem"/>.
    /// PreferredRawMaterial FK uses Restrict so deleting a material does not
    /// cascade into and destroy the production request history.
    /// </summary>
    public class BoProductionRequestItemConfiguration : IEntityTypeConfiguration<BoProductionRequestItem>
    {
        public void Configure(EntityTypeBuilder<BoProductionRequestItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductType)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Quantity)
                .IsRequired();

            builder.Property(i => i.Specifications)
                .HasMaxLength(1000);

            builder.HasOne(i => i.PreferredRawMaterial)
                .WithMany()
                .HasForeignKey(i => i.PreferredRawMaterialId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}