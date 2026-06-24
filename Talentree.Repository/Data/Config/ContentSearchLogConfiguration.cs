using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class ContentSearchLogConfiguration : IEntityTypeConfiguration<ContentSearchLog>
    {
        public void Configure(EntityTypeBuilder<ContentSearchLog> builder)
        {
            builder.ToTable("ContentSearchLogs");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SearchTerm)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(s => s.SearchedAt)
                .IsRequired();

            builder.Property(s => s.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // FK to AppUser (no cascade delete — retain logs even if user is deleted)
            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Index for analytics queries (group by term, order by date)
            builder.HasIndex(s => s.SearchedAt);
            builder.HasIndex(s => s.SearchTerm);
        }
    }
}
