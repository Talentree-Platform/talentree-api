using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class UserActionLogConfiguration : IEntityTypeConfiguration<UserActionLog>
    {
        public void Configure(EntityTypeBuilder<UserActionLog> builder)
        {
            builder.ToTable("UserActionLogs");
            builder.HasKey(l => l.Id);

            builder.Property(l => l.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(l => l.AdminId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(l => l.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(l => l.Reason)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(l => l.Notes)
                .HasColumnType("nvarchar(max)");

            builder.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Admin)
                .WithMany()
                .HasForeignKey(l => l.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(l => l.UserId)
                .HasDatabaseName("IX_UserActionLogs_UserId");

            builder.HasIndex(l => l.AdminId)
                .HasDatabaseName("IX_UserActionLogs_AdminId");

            builder.HasIndex(l => l.ActionDate)
                .HasDatabaseName("IX_UserActionLogs_ActionDate");
        }
    }
}