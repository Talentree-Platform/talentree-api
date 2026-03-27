using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for <see cref="BoProductionRequest"/>.
    /// Stores Status as a string. Indexes BusinessOwnerId and Status
    /// for fast per-BO listing and Admin inbox filtering respectively.
    /// </summary>
    public class BoProductionRequestConfiguration : IEntityTypeConfiguration<BoProductionRequest>
    {
        public void Configure(EntityTypeBuilder<BoProductionRequest> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(r => r.Notes)
                .HasMaxLength(3000);

            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(r => r.QuotedPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.AdminNotes)
                .HasMaxLength(2000);

            builder.Property(r => r.StripePaymentIntentId)
                .HasMaxLength(300);

            builder.Property(r => r.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(PaymentStatus.Unpaid);

            // Fast lookup for BO's own request list
            builder.HasIndex(r => r.BusinessOwnerId);

            // Fast filtering for Admin inbox by status
            builder.HasIndex(r => r.Status);

            builder.HasMany(r => r.Items)
                .WithOne(i => i.Request)
                .HasForeignKey(i => i.BoProductionRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.StatusHistory)
                .WithOne(h => h.Request)
                .HasForeignKey(h => h.BoProductionRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}