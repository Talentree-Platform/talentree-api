using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// EF Core configuration for <see cref="MaterialOrder"/>.
    /// Stores Status as a string for readability in the database.
    /// Indexes BusinessOwnerId for fast per-BO order history queries.
    /// </summary>
    public class MaterialOrderConfiguration : IEntityTypeConfiguration<MaterialOrder>
    {
        public void Configure(EntityTypeBuilder<MaterialOrder> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(o => o.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.DeliveryCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.DeliveryCountry)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ContactPhone)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(o => o.PaymentMethod)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.StripePaymentIntentId)
                .HasMaxLength(300);

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Fast lookup for BO order history
            builder.HasIndex(o => o.BusinessOwnerId);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.MaterialOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}