using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-32: EF Core configuration for CommissionSetting.
    /// Single-row table — application always reads/writes row with Id = 1.
    /// </summary>
    public class CommissionSettingConfiguration : AuditableEntityConfiguration<CommissionSetting>
    {
        public override void Configure(EntityTypeBuilder<CommissionSetting> builder)
        {
            base.Configure(builder);
            builder.ToTable("CommissionSettings");

            builder.Property(c => c.PlatformCommissionPercent)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(c => c.TransactionFeeValue)
                .HasPrecision(10, 4)
                .HasDefaultValue(0);

            builder.Property(c => c.IsTransactionFeePercent)
                .HasDefaultValue(true);

            builder.Property(c => c.MinimumPayoutAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(c => c.PayoutProcessingFee)
                .HasPrecision(18, 4)
                .HasDefaultValue(0);
        }
    }
}
