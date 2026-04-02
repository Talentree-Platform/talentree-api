using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// EF Core configuration for <see cref="Transaction"/>.
    /// Append-only ledger — no cascade deletes.
    /// Indexed on BusinessOwnerId + CreatedAt for fast dashboard queries.
    /// </summary>
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(t => t.Type)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(t => t.Amount)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(t => t.BalanceAfter)
                .HasColumnType("decimal(14,2)")
                .IsRequired();

            builder.Property(t => t.ReferenceType)
                .HasMaxLength(50);

            builder.Property(t => t.StripePaymentIntentId)
                .HasMaxLength(300);

            // AI columns — always inserted as defaults, updated by ML pipeline
            builder.Property(t => t.AnomalyFlag)
                .HasDefaultValue(false);

            builder.Property(t => t.AnomalyScore)
                .HasColumnType("real");

            // Fast dashboard queries: all transactions for a BO ordered by date
            builder.HasIndex(t => new { t.BusinessOwnerId, t.CreatedAt });

            // Fast lookup by Stripe intent for reconciliation
            builder.HasIndex(t => t.StripePaymentIntentId);
        }
    }
}
