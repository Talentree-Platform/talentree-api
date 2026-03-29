using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;

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
    }
}
