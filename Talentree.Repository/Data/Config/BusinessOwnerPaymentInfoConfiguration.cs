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
    public class BusinessOwnerPaymentInfoConfiguration : BaseEntityConfiguration<BusinessOwnerPaymentInfo>
    {
        public override void Configure(EntityTypeBuilder<BusinessOwnerPaymentInfo> builder)
        {
            base.Configure(builder);
            builder.ToTable("BusinessOwnerPaymentInfos");

            builder.Property(p => p.BankName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.AccountHolderName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.AccountNumberEncrypted)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.RoutingSwiftCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.LastChangedByIp)
                .HasMaxLength(50);

            builder.HasIndex(p => p.UserId).IsUnique();
        }
    }
}
