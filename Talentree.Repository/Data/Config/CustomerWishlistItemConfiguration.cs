using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class CustomerWishlistItemConfiguration : BaseEntityConfiguration<CustomerWishlistItem>
    {
        public override void Configure(EntityTypeBuilder<CustomerWishlistItem> builder)
        {
            base.Configure(builder);
            builder.HasKey(i => i.Id);

            builder.Property(i => i.AddedAt).HasDefaultValueSql("GETUTCDATE()").IsRequired();

            // Unique product per wishlist
            builder.HasIndex(i => new { i.WishlistId, i.ProductId })
                .IsUnique();

            builder.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
