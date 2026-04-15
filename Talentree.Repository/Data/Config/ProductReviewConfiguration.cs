using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class ProductReviewConfiguration : BaseEntityConfiguration<ProductReview>
    {
        public override void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            base.Configure(builder);
            builder.ToTable("ProductReviews");

            builder.Property(r => r.CustomerUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.CustomerName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.SentimentLabel)
                .HasMaxLength(20);

            builder.Property(r => r.FlaggedToxic)
                .HasDefaultValue(false);

            builder.Property(r => r.IsAnonymous)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.CustomerUserId);
        }
    }
}