using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class OrderStatusHistoryConfiguration : BaseEntityConfiguration<OrderStatusHistory>
    {
        public override void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            base.Configure(builder);
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Status).HasConversion<int>().IsRequired();
            builder.Property(h => h.Notes).HasMaxLength(500);
            builder.Property(h => h.ChangedBy).HasMaxLength(150);
            builder.Property(h => h.ChangedAt).HasDefaultValueSql("GETUTCDATE()").IsRequired();
        }
    }
}
