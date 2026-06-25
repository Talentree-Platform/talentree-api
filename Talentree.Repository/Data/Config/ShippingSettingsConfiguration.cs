using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-33: EF Core configuration for ShippingSettings.
    /// Single-row table — application always reads/writes row with Id = 1.
    /// </summary>
    public class ShippingSettingsConfiguration : AuditableEntityConfiguration<ShippingSettings>
    {
        public override void Configure(EntityTypeBuilder<ShippingSettings> builder)
        {
            base.Configure(builder);
            builder.ToTable("ShippingSettings");

            builder.Property(s => s.FlatRate)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.FreeShippingThreshold)
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(s => s.FreeShippingEnabled)
                .HasDefaultValue(false);

            builder.Property(s => s.IsFlatRatePerItem)
                .HasDefaultValue(false);

            builder.Property(s => s.InternationalShippingEnabled)
                .HasDefaultValue(false);

            builder.Property(s => s.EstimatedDeliveryDomesticDays)
                .HasDefaultValue(3);

            builder.Property(s => s.EstimatedDeliveryInternationalDays)
                .HasDefaultValue(14);
        }
    }
}
