using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class CustomerCartItemConfiguration : BaseEntityConfiguration<CustomerCartItem>
    {
        public override void Configure(EntityTypeBuilder<CustomerCartItem> builder)
        {
            base.Configure(builder);
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity)
                .IsRequired();

            // Ensure distinct product per cart
            builder.HasIndex(i => new { i.CartId, i.ProductId })
                .IsUnique();

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Deleting product is protected or restricted
        }
    }
}
