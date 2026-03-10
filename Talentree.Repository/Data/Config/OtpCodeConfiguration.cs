using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;

public class OtpCodeConfiguration : BaseEntityConfiguration<OtpCode>
{
    public  override void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        base.Configure(builder);
        builder.ToTable("OtpCodes");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(o => o.Purpose)
            .IsRequired();

        builder.Property(o => o.IsUsed)
            .HasDefaultValue(false);

        builder.HasIndex(o => o.UserId);
    }
}
