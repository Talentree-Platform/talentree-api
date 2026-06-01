using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.Config
{
    public class NotificationPreferenceConfig : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.HasKey(np => np.Id);

            // User relationship (One-to-One)
            builder.HasOne(np => np.User)
                .WithOne()
                .HasForeignKey<NotificationPreference>(np => np.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Table name
            builder.ToTable("NotificationPreferences");

            // Timestamps
            builder.Property(np => np.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.Property(np => np.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAddOrUpdate();

            // ✅ ═══════════════════════════════════════════════════════════
            // EXPLICIT DATABASE DEFAULTS - IMPORTANT!
            // ✅ ═══════════════════════════════════════════════════════════

            // All notification types should default to TRUE
            builder.Property(np => np.ReceiveAccountNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveOrderNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveProductNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveSupportNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceivePaymentNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveReviewNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveProductionNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceivePayoutNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveComplaintNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveAutoBlockNotifications)
                .HasDefaultValue(true);

            // Channel preferences default to TRUE
            builder.Property(np => np.ReceiveInApp)
                .HasDefaultValue(true);

            builder.Property(np => np.ReceiveEmail)
                .HasDefaultValue(true);

            // Push is FALSE by default
            builder.Property(np => np.ReceivePush)
                .HasDefaultValue(false);

            // Priority settings
            builder.Property(np => np.MinimumPriority)
                .HasDefaultValue(NotificationPriority.Low);

            builder.Property(np => np.EnableQuietHours)
                .HasDefaultValue(false);

            // Indexes
            builder.HasIndex(np => np.UserId)
                .IsUnique()
                .HasDatabaseName("IX_NotificationPreference_UserId_Unique");

            // Index for filtering by user and preference
            builder.HasIndex(np => new { np.UserId, np.CreatedAt });
        }
    }
}