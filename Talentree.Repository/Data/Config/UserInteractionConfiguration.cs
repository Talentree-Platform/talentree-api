
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Core.EntityConfigurations
{
    public class UserInteractionConfiguration : IEntityTypeConfiguration<UserInteraction>
    {
        public void Configure(EntityTypeBuilder<UserInteraction> builder)
        {
            builder.HasKey(ui => ui.Id);

            // User ID
            builder.Property(ui => ui.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // User Type
            builder.Property(ui => ui.UserType)
                .IsRequired()
                .HasConversion<int>();

            // Item ID
            builder.Property(ui => ui.ItemId)
                .IsRequired();

            // Item Type
            builder.Property(ui => ui.ItemType)
                .IsRequired()
                .HasConversion<int>();

            // Action Type
            builder.Property(ui => ui.ActionType)
                .IsRequired()
                .HasConversion<int>();

            // Category
            builder.Property(ui => ui.Category)
                .IsRequired()
                .HasMaxLength(100);

            // Quantity
            builder.Property(ui => ui.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            // Price
            builder.Property(ui => ui.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            // Timestamps
            builder.Property(ui => ui.InteractionTimestamp)
                .IsRequired();

            builder.Property(ui => ui.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign Key
            builder.HasOne(ui => ui.User)
                .WithMany()
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for AI queries
            builder.HasIndex(ui => ui.UserId)
                .HasDatabaseName("IX_UserInteraction_UserId");

            builder.HasIndex(ui => new { ui.UserId, ui.ActionType })
                .HasDatabaseName("IX_UserInteraction_UserId_ActionType");

            builder.HasIndex(ui => ui.InteractionTimestamp)
                .HasDatabaseName("IX_UserInteraction_InteractionTimestamp");

            builder.HasIndex(ui => new { ui.UserId, ui.CreatedAt })
                .HasDatabaseName("IX_UserInteraction_UserId_CreatedAt");

            // Table name
            builder.ToTable("UserInteractions", "dbo");
        }
    }
}