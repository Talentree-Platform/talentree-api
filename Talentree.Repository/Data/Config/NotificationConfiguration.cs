using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// Configuration for Notification entity
    /// Defines table structure, relationships, and indexes
    /// </summary>
    public class NotificationConfig : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // ═══════════════════════════════════════════════════════════
            // PRIMARY KEY
            // ═══════════════════════════════════════════════════════════
            builder.HasKey(n => n.Id);

            // ═══════════════════════════════════════════════════════════
            // PROPERTIES
            // ═══════════════════════════════════════════════════════════

            builder.Property(n => n.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(n => n.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(n => n.ActionUrl)
                .HasMaxLength(500);

            builder.Property(n => n.ActionText)
                .HasMaxLength(100);

            builder.Property(n => n.RelatedEntityType)
                .HasMaxLength(100);

            builder.Property(n => n.Priority)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(NotificationPriority.Normal);

            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // ═══════════════════════════════════════════════════════════
            // RELATIONSHIPS
            // ═══════════════════════════════════════════════════════════

            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ═══════════════════════════════════════════════════════════
            // INDEXES (for performance)
            // ═══════════════════════════════════════════════════════════

            // Search notifications by user
            builder.HasIndex(n => n.UserId)
                .HasDatabaseName("IX_Notification_UserId");

            // Find unread notifications quickly
            builder.HasIndex(n => new { n.UserId, n.IsRead })
                .HasDatabaseName("IX_Notification_UserId_IsRead");

            // Sort by creation date
            builder.HasIndex(n => n.CreatedAt)
                .HasDatabaseName("IX_Notification_CreatedAt");

            // Combined search
            builder.HasIndex(n => new { n.UserId, n.CreatedAt })
                .HasDatabaseName("IX_Notification_UserId_CreatedAt");

            // Find related entity notifications
            builder.HasIndex(n => new { n.RelatedEntityType, n.RelatedEntityId })
                .HasDatabaseName("IX_Notification_RelatedEntity");

            // ═══════════════════════════════════════════════════════════
            // TABLE NAME
            // ═══════════════════════════════════════════════════════════

            builder.ToTable("Notifications");
        }
    }
}