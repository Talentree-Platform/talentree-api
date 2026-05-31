using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class ArticleBookmarkConfiguration : BaseEntityConfiguration<ArticleBookmark>
    {
        public override void Configure(EntityTypeBuilder<ArticleBookmark> builder)
        {
            base.Configure(builder);
            builder.ToTable("ArticleBookmarks");

            builder.Property(b => b.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // One bookmark per user per article
            builder.HasIndex(b => new { b.UserId, b.ArticleId }).IsUnique();

            // Relationships
            builder.HasOne(b => b.Article)
                .WithMany(a => a.Bookmarks)
                .HasForeignKey(b => b.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}