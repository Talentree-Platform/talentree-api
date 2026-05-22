using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class ReviewPhotoConfiguration : BaseEntityConfiguration<ReviewPhoto>
    {
        public override void Configure(EntityTypeBuilder<ReviewPhoto> builder)
        {
            base.Configure(builder);
            builder.ToTable("ReviewPhotos");

            builder.Property(rp => rp.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rp => rp.SortOrder)
                .HasDefaultValue(0);

            builder.HasOne(rp => rp.Review)
                .WithMany(r => r.Photos)
                .HasForeignKey(rp => rp.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rp => rp.ReviewId);
        }
    }
}
