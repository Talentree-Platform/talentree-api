using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-35: EF Core configuration for HomepageBanner.
    /// </summary>
    public class HomepageBannerConfiguration : AuditableEntityConfiguration<HomepageBanner>
    {
        public override void Configure(EntityTypeBuilder<HomepageBanner> builder)
        {
            base.Configure(builder);
            builder.ToTable("HomepageBanners");

            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Subtitle)
                .HasMaxLength(500);

            builder.Property(b => b.ImageUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(b => b.LinkUrl)
                .HasMaxLength(2048);

            builder.Property(b => b.TextOverlay)
                .HasMaxLength(500);

            builder.Property(b => b.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(b => b.IsHero)
                .HasDefaultValue(false);

            builder.Property(b => b.IsActive)
                .HasDefaultValue(true);

            // Index to efficiently fetch active banners ordered by display order
            builder.HasIndex(b => new { b.IsActive, b.DisplayOrder })
                .HasDatabaseName("IX_HomepageBanners_IsActive_DisplayOrder");

            // Index to support schedule-based filtering
            builder.HasIndex(b => new { b.ScheduleStart, b.ScheduleEnd })
                .HasDatabaseName("IX_HomepageBanners_Schedule");
        }
    }
}
