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
    public class LoginHistoryConfiguration : BaseEntityConfiguration<LoginHistory>
    {
        public override void Configure(EntityTypeBuilder<LoginHistory> builder)
        {
            base.Configure(builder);
            builder.ToTable("LoginHistories");

            builder.Property(l => l.IpAddress).IsRequired().HasMaxLength(50);
            builder.Property(l => l.DeviceInfo).HasMaxLength(500);
            builder.Property(l => l.Location).HasMaxLength(200);

            builder.HasIndex(l => l.UserId);
            builder.HasIndex(l => l.LoginAt);
        }
    }
}
