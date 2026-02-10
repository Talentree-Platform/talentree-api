using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;

public class BusinessOwnerProfileConfiguration
    : BaseEntityConfiguration<BusinessOwnerProfile>
{
    public override void Configure(EntityTypeBuilder<BusinessOwnerProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("BusinessOwnerProfile");

        builder.Property(b => b.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.BusinessDescription)
            .HasMaxLength(2000);

        builder.Property(b => b.BusinessCategory)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(b => b.Status)
            .IsRequired();

        builder.HasIndex(b => b.UserId)
            .IsUnique();
    }
}
