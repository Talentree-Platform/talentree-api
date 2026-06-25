using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-35: EF Core configuration for HomepageFeaturedProduct.
    /// </summary>
    public class HomepageFeaturedProductConfiguration : AuditableEntityConfiguration<HomepageFeaturedProduct>
    {
        public override void Configure(EntityTypeBuilder<HomepageFeaturedProduct> builder)
        {
            base.Configure(builder);
            builder.ToTable("HomepageFeaturedProducts");

            builder.Property(p => p.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            // Each product can only be featured once at a time
            builder.HasIndex(p => p.ProductId)
                .IsUnique()
                .HasDatabaseName("IX_HomepageFeaturedProducts_ProductId_Unique");

            // FK to Products — restrict delete to avoid silent removal from featured list
            builder.HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for schedule-based filtering
            builder.HasIndex(p => new { p.ScheduleStart, p.ScheduleEnd })
                .HasDatabaseName("IX_HomepageFeaturedProducts_Schedule");
        }
    }
}
