using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class AiImageConfiguration : IEntityTypeConfiguration<AiImage>
    {
        public void Configure(EntityTypeBuilder<AiImage> builder)
        {
            builder.ToTable("AiImages");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.StorageUrl)
                .IsRequired()
                .HasMaxLength(2083);

            builder.Property(i => i.R2ObjectKey)
                .IsRequired()
                .HasMaxLength(1024);

            builder.Property(i => i.EnhancedPrompt)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(i => i.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship with AiMessage
            builder.HasOne(i => i.Message)
                .WithMany(m => m.GeneratedImages)
                .HasForeignKey(i => i.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(i => i.MessageId);
        }
    }
}
