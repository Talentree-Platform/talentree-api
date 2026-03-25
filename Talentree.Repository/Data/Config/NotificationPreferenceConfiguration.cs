
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
  
    public class NotificationPreferenceConfiguration : AuditableEntityConfiguration<NotificationPreference>
    {
        public override void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            // Apply base configuration (audit only, no soft delete)
            base.Configure(builder);

            // ═══════════════════════════════════════════════════════════
            // TABLE
            // ═══════════════════════════════════════════════════════════

            builder.ToTable("NotificationPreferences");

            // ═══════════════════════════════════════════════════════════
            // PROPERTIES
            // ═══════════════════════════════════════════════════════════

            builder.Property(np => np.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // In-app preferences (default: true)
            builder.Property(np => np.EnableSystemNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.EnableOrderNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.EnableFinancialNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.EnableSupportNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.EnableProductNotifications)
                .HasDefaultValue(true);

            // Email preferences
            builder.Property(np => np.EmailSystemNotifications)
                .HasDefaultValue(false);

            builder.Property(np => np.EmailOrderNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.EmailFinancialNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.EmailSupportNotifications)
                .HasDefaultValue(true);

            builder.Property(np => np.EmailProductNotifications)
                .HasDefaultValue(true);

            // Real-time
            builder.Property(np => np.EnableRealTimeNotifications)
                .HasDefaultValue(true);

            // Quiet hours
            builder.Property(np => np.EnableQuietHours)
                .HasDefaultValue(false);

            // Sound
            builder.Property(np => np.NotificationSound)
                .HasMaxLength(50)
                .HasDefaultValue("default");

            builder.Property(np => np.EnableSound)
                .HasDefaultValue(true);

            // ═══════════════════════════════════════════════════════════
            // RELATIONSHIPS
            // ═══════════════════════════════════════════════════════════

            builder.HasOne(np => np.User)
                .WithOne() // One-to-one
                .HasForeignKey<NotificationPreference>(np => np.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ═══════════════════════════════════════════════════════════
            // INDEXES
            // ═══════════════════════════════════════════════════════════

            builder.HasIndex(np => np.UserId)
                .IsUnique() // One preference per user
                .HasDatabaseName("IX_NotificationPreferences_UserId");
        }
    }
}