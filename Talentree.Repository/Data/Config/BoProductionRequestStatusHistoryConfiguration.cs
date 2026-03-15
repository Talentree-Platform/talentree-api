using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Configurations
{
    /// <summary>
    /// EF Core configuration for <see cref="BoProductionRequestStatusHistory"/>.
    /// History records are append-only — never updated or deleted directly.
    /// </summary>
    public class BoProductionRequestStatusHistoryConfiguration
        : IEntityTypeConfiguration<BoProductionRequestStatusHistory>
    {
        public void Configure(EntityTypeBuilder<BoProductionRequestStatusHistory> builder)
        {
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(h => h.ChangedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(h => h.Notes)
                .HasMaxLength(1000);
        }
    }
}