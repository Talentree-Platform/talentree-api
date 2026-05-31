using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class CustomerWishlistConfiguration : AuditableEntityConfiguration<CustomerWishlist>
    {
        public override void Configure(EntityTypeBuilder<CustomerWishlist> builder)
        {
            base.Configure(builder);
            builder.HasKey(w => w.Id);

            builder.Property(w => w.CustomerId)
                .IsRequired()
                .HasMaxLength(450);

            // One wishlist per customer
            builder.HasIndex(w => w.CustomerId)
                .IsUnique();

            builder.HasMany(w => w.Items)
                .WithOne(i => i.Wishlist)
                .HasForeignKey(i => i.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
