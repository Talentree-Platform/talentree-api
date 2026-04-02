using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities.Identity;

namespace Talentree.Repository.Data.Config
{
    public class UserPreferencesConfiguration : BaseEntityConfiguration<UserPreferences>
    {
        public override void Configure(EntityTypeBuilder<UserPreferences> builder)
        {
            base.Configure(builder);
            builder.ToTable("UserPreferences");

            builder.Property(p => p.Timezone).HasMaxLength(100).HasDefaultValue("Africa/Cairo");
            builder.Property(p => p.DateFormat).HasMaxLength(20).HasDefaultValue("dd/MM/yyyy");
            builder.Property(p => p.CurrencyDisplay).HasMaxLength(10).HasDefaultValue("EGP");
            builder.Property(p => p.DashboardLayout).HasMaxLength(50).HasDefaultValue("default");
            builder.Property(p => p.Language).HasMaxLength(10);

            builder.HasIndex(p => p.UserId).IsUnique();
        }
    }
}
