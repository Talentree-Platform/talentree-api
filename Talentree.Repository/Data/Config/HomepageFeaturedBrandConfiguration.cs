using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-35: EF Core configuration for HomepageFeaturedBrand.
    /// </summary>
    public class HomepageFeaturedBrandConfiguration : AuditableEntityConfiguration<HomepageFeaturedBrand>
    {
        public override void Configure(EntityTypeBuilder<HomepageFeaturedBrand> builder)
        {
            base.Configure(builder);
            builder.ToTable("HomepageFeaturedBrands");

            builder.Property(b => b.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(b => b.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(b => b.IsActive)
                .HasDefaultValue(true);

            // Each business owner can only appear once in the featured brands list
            builder.HasIndex(b => b.BusinessOwnerId)
                .IsUnique()
                .HasDatabaseName("IX_HomepageFeaturedBrands_BusinessOwnerId_Unique");

            // FK to AspNetUsers — restrict so deleting a user doesn't cascade-drop the feature
            builder.HasOne(b => b.BusinessOwner)
                .WithMany()
                .HasForeignKey(b => b.BusinessOwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
