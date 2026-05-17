using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.EmailConfirmed)
            .HasDefaultValue(false);

        // 1 : 1 Address
        builder.HasOne(u => u.Address)
            .WithOne(a => a.User)
            .HasForeignKey<Address>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1 : 1 BusinessOwnerProfile
        builder.HasOne(u => u.BusinessOwnerProfile)
            .WithOne(b => b.User)
            .HasForeignKey<BusinessOwnerProfile>(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 PaymentInfo
        builder.HasOne(u => u.PaymentInfo)
            .WithOne(p => p.User)
            .HasForeignKey<BusinessOwnerPaymentInfo>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 Preferences
        builder.HasOne(u => u.Preferences)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:Many LoginHistories
        builder.HasMany(u => u.LoginHistories)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User Mangement Properties
        builder.Property(u => u.AccountStatus)
            .HasConversion<int>()
            .HasDefaultValue(AccountStatus.Active);

        builder.Property(u => u.SuspensionReason)
            .HasMaxLength(500);

        builder.Property(u => u.BanReason)
            .HasMaxLength(500);

        builder.Property(u => u.BlockReason)
            .HasMaxLength(500);

        builder.Property(u => u.SuspendedBy)
            .HasMaxLength(450);

        builder.Property(u => u.BannedBy)
            .HasMaxLength(450);

        builder.Property(u => u.BlockedBy)
            .HasMaxLength(450);

        // Indexes
        builder.HasIndex(u => u.AccountStatus)
            .HasDatabaseName("IX_AppUser_AccountStatus");

        builder.HasIndex(u => u.LastLoginAt)
            .HasDatabaseName("IX_AppUser_LastLoginAt");

        builder.HasIndex(u => u.IsBlocked)
            .HasDatabaseName("IX_AppUser_IsBlocked");
    }
}
