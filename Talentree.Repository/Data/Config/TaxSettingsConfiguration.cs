using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-34: EF Core configuration for TaxSettings.
    /// Single-row table — application always reads/writes row with Id = 1.
    /// </summary>
    public class TaxSettingsConfiguration : AuditableEntityConfiguration<TaxSettings>
    {
        public override void Configure(EntityTypeBuilder<TaxSettings> builder)
        {
            base.Configure(builder);
            builder.ToTable("TaxSettings");

            builder.Property(t => t.TaxEnabled)
                .HasDefaultValue(false);

            builder.Property(t => t.TaxRate)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(t => t.IsInclusive)
                .HasDefaultValue(false);

            builder.Property(t => t.TaxExemptCategoryIds)
                .HasMaxLength(1000);
        }
    }
}
