using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class FAQConfiguration : SoftDeleteEntityConfiguration<FAQ>
    {
        public override void Configure(EntityTypeBuilder<FAQ> builder)
        {
            base.Configure(builder);

            builder.ToTable("FAQs");

            builder.Property(f => f.Question)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.Answer)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(f => f.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(f => f.IsPublished)
                .HasDefaultValue(true);

            builder.Property(f => f.ViewCount)
                .HasDefaultValue(0);

            builder.Property(f => f.Keywords)
                .HasMaxLength(500);

            builder.Property(f => f.RelatedFaqIds)
                .HasMaxLength(500);

            builder.HasIndex(f => f.Category)
                .HasDatabaseName("IX_FAQs_Category");

            builder.HasIndex(f => new { f.IsPublished, f.Category, f.DisplayOrder })
                .HasDatabaseName("IX_FAQs_IsPublished_Category_DisplayOrder");
        }
    }
}