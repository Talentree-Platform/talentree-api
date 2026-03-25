
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    
    public class NotificationConfiguration : AuditableEntityConfiguration<Notification>
    {
        public override void Configure(EntityTypeBuilder<Notification> builder)
        {
            // Apply base configuration (audit + soft delete)
            base.Configure(builder);

            // ═══════════════════════════════════════════════════════════
            // TABLE
            // ═══════════════════════════════════════════════════════════

            builder.ToTable("Notifications");

            // ═══════════════════════════════════════════════════════════
            // PROPERTIES
            // ═══════════════════════════════════════════════════════════

            // User
            builder.Property(n => n.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // Content
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(n => n.ActionUrl)
                .HasMaxLength(500);

            builder.Property(n => n.ActionText)
                .HasMaxLength(50);

            // Metadata
            builder.Property(n => n.RelatedEntityType)
                .HasMaxLength(50);

            builder.Property(n => n.Data)
                .HasColumnType("nvarchar(max)"); // JSON data

            // Type & Priority (stored as int)
            builder.Property(n => n.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(n => n.Priority)
                .HasConversion<int>()
                .HasDefaultValue(NotificationPriority.Normal);

            // Status
            builder.Property(n => n.IsRead)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(n => n.EmailSent)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(n => n.RealTimeSent)
                .HasDefaultValue(false)
                .IsRequired();

            // ═══════════════════════════════════════════════════════════
            // RELATIONSHIPS
            // ═══════════════════════════════════════════════════════════

            builder.HasOne(n => n.User)
                .WithMany() // User doesn't need navigation to notifications
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete notifications when user deleted

            // ═══════════════════════════════════════════════════════════
            // INDEXES (Critical for Performance)
            // ═══════════════════════════════════════════════════════════

            // Most common query: Get user's unread notifications
            builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt })
                .HasDatabaseName("IX_Notifications_UserId_IsRead_CreatedAt")
                .IncludeProperties(n => new { n.Type, n.Title }); // Covering index

            // Filter by type
            builder.HasIndex(n => new { n.UserId, n.Type })
                .HasDatabaseName("IX_Notifications_UserId_Type");

            // Clean up expired notifications
            builder.HasIndex(n => n.ExpiresAt)
                .HasDatabaseName("IX_Notifications_ExpiresAt")
                .HasFilter("[ExpiresAt] IS NOT NULL");

            // Related entity lookup
            builder.HasIndex(n => new { n.RelatedEntityType, n.RelatedEntityId })
                .HasDatabaseName("IX_Notifications_RelatedEntity")
                .HasFilter("[RelatedEntityType] IS NOT NULL");
        }
    }
}