using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;

public class RefreshTokenConfiguration
    : BaseEntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("RefreshTokens");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(r => r.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        builder.HasIndex(r => r.TokenHash)
            .IsUnique();

        builder.HasIndex(r => r.UserId);
    }
}
