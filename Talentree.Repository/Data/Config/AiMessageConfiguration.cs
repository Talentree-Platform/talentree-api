using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
    {
        public void Configure(EntityTypeBuilder<AiMessage> builder)
        {
            builder.ToTable("AiMessages");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Role)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship with AiSession
            builder.HasOne(m => m.Session)
                .WithMany(s => s.Messages)
                .HasForeignKey(m => m.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => m.SessionId);
            builder.HasIndex(m => new { m.SessionId, m.CreatedAt });
        }
    }
}
