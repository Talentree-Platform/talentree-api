using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class AiSessionConfiguration : IEntityTypeConfiguration<AiSession>
    {
        public void Configure(EntityTypeBuilder<AiSession> builder)
        {
            builder.ToTable("AiSessions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Relationship with BusinessOwnerProfile
            builder.HasOne(s => s.BusinessOwnerProfile)
                .WithMany()
                .HasForeignKey(s => s.BusinessOwnerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.BusinessOwnerProfileId);
        }
    }
}
