using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;

namespace Talentree.Repository.Data.Config
{
    public class SecuritySettingsConfiguration : IEntityTypeConfiguration<SecuritySettings>
    {
        public void Configure(EntityTypeBuilder<SecuritySettings> builder)
        {
            builder.ToTable("SecuritySettings");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.PasswordRequiredLength).IsRequired();
            builder.Property(s => s.PasswordRequireDigit).IsRequired();
            builder.Property(s => s.PasswordRequireLowercase).IsRequired();
            builder.Property(s => s.PasswordRequireUppercase).IsRequired();
            builder.Property(s => s.PasswordRequireNonAlphanumeric).IsRequired();

            builder.Property(s => s.SessionTimeoutInMinutes).IsRequired();
            builder.Property(s => s.MaxFailedAccessAttempts).IsRequired();
            builder.Property(s => s.LockoutDurationInMinutes).IsRequired();
            builder.Property(s => s.RequireTwoFactorForAdmins).IsRequired();

            builder.Property(s => s.IpWhitelist)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(s => s.AllowedLoginStartTime)
                .IsRequired(false);

            builder.Property(s => s.AllowedLoginEndTime)
                .IsRequired(false);
        }
    }
}
