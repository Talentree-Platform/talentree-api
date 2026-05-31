using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class CustomerOrderConfiguration : AuditableEntityConfiguration<CustomerOrder>
    {
        public override void Configure(EntityTypeBuilder<CustomerOrder> builder)
        {
            base.Configure(builder);
            builder.HasKey(o => o.Id);

            builder.Property(o => o.CustomerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(o => o.FullName).IsRequired().HasMaxLength(150);
            builder.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(50);
            builder.Property(o => o.Street).IsRequired().HasMaxLength(250);
            builder.Property(o => o.City).IsRequired().HasMaxLength(100);
            builder.Property(o => o.PostalCode).IsRequired().HasMaxLength(20);
            builder.Property(o => o.Country).IsRequired().HasMaxLength(100);

            builder.Property(o => o.SubtotalAmount).HasPrecision(18, 2);
            builder.Property(o => o.ShippingAmount).HasPrecision(18, 2);
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);

            builder.Property(o => o.StripePaymentIntentId).HasMaxLength(250);
            builder.Property(o => o.TrackingNumber).HasMaxLength(100);
            builder.Property(o => o.AdminNotes).HasMaxLength(500);

            builder.Property(o => o.Status).HasConversion<int>();
            builder.Property(o => o.PaymentStatus).HasConversion<int>();
            builder.Property(o => o.PaymentMethod).HasConversion<int>();

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.StatusHistory)
                .WithOne(h => h.Order)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
