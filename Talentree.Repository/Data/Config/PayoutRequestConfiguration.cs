using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// EF Core configuration for <see cref="PayoutRequest"/>.
    /// One pending request per BO enforced via unique filtered index.
    /// Bank account identifier is stored encrypted — never plaintext.
    /// </summary>
    public class PayoutRequestConfiguration : IEntityTypeConfiguration<PayoutRequest>
    {
        public void Configure(EntityTypeBuilder<PayoutRequest> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("EGP");

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(p => p.BankName)
                .HasMaxLength(150);

            builder.Property(p => p.AccountHolderName)
                .HasMaxLength(150);

            builder.Property(p => p.AccountIdentifierEnc)
                .HasMaxLength(500);

            builder.Property(p => p.RoutingSwiftCode)
                .HasMaxLength(50);

            builder.Property(p => p.ProcessedBy)
                .HasMaxLength(450);

            // Enforce: only one Pending payout per BO at a time
            // Filtered unique index — only applies to Pending status
            builder.HasIndex(p => new { p.BusinessOwnerId, p.Status })
                .HasFilter("[Status] = 'Pending'")
                .IsUnique();

            // Fast lookup for Admin inbox
            builder.HasIndex(p => p.Status);
        }
    }
}