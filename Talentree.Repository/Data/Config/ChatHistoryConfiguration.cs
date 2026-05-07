using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class ChatHistoryConfiguration : IEntityTypeConfiguration<ChatHistory>
    {
        public void Configure(EntityTypeBuilder<ChatHistory> builder)
        {
            builder.ToTable("ChatHistory");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Role)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Agent)
                .HasMaxLength(50);

            builder.Property(c => c.Message)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // ✅ Relationship with BusinessOwnerProfile
            builder.HasOne(c => c.BusinessOwnerProfile)
                .WithMany()
                .HasForeignKey(c => c.BusinessOwnerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.BusinessOwnerProfileId);
            builder.HasIndex(c => c.CreatedAt);
            builder.HasIndex(c => new { c.BusinessOwnerProfileId, c.CreatedAt });
            builder.HasIndex(c => c.Agent);
        }
    }
}