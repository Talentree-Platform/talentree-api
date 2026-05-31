using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class KnowledgeArticleConfiguration : BaseEntityConfiguration<KnowledgeArticle>
    {
        public override void Configure(EntityTypeBuilder<KnowledgeArticle> builder)
        {
            base.Configure(builder);
            builder.ToTable("KnowledgeArticles");

            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Summary)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.ExternalUrl)
                .HasMaxLength(500);

            builder.Property(a => a.FileUrl)
                .HasMaxLength(500);

            builder.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Tags)
                .HasMaxLength(500);

            builder.Property(a => a.ThumbnailUrl)
                .HasMaxLength(500);

            builder.Property(a => a.IsPublished)
                .HasDefaultValue(false);

            builder.Property(a => a.ViewCount)
                .HasDefaultValue(0);

            builder.Property(a => a.OrderIndex)
                .HasDefaultValue(0);

            // Indexes
            builder.HasIndex(a => a.Category);
            builder.HasIndex(a => a.ContentType);
            builder.HasIndex(a => a.IsPublished);
        }
    }
}