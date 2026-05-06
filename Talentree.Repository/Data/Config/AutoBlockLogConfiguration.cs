using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class AutoBlockLogConfiguration : IEntityTypeConfiguration<AutoBlockLog>
    {
        public void Configure(EntityTypeBuilder<AutoBlockLog> builder)
        {
            builder.ToTable("AutoBlockLogs");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(l => l.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.ReviewedBy)
                .HasMaxLength(450);

            builder.Property(l => l.AdminDecision)
                .HasMaxLength(100);

            builder.Property(l => l.AdminNotes)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(l => l.UserId)
                .HasDatabaseName("IX_AutoBlockLogs_UserId");

            builder.HasIndex(l => l.IsReviewed)
                .HasDatabaseName("IX_AutoBlockLogs_IsReviewed");

            builder.HasIndex(l => l.BlockedAt)
                .HasDatabaseName("IX_AutoBlockLogs_BlockedAt");
        }
    }
}