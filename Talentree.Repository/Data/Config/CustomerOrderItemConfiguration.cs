using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class CustomerOrderItemConfiguration : BaseEntityConfiguration<CustomerOrderItem>
    {
        public override void Configure(EntityTypeBuilder<CustomerOrderItem> builder)
        {
            base.Configure(builder);
            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductName).IsRequired().HasMaxLength(200);
            builder.Property(i => i.ProductImageUrl).HasMaxLength(500);
            builder.Property(i => i.SellerName).IsRequired().HasMaxLength(150);
            builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
            builder.Property(i => i.Quantity).IsRequired();

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
