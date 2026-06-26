using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-35: EF Core configuration for AnnouncementBar.
    /// </summary>
    public class AnnouncementBarConfiguration : AuditableEntityConfiguration<AnnouncementBar>
    {
        public override void Configure(EntityTypeBuilder<AnnouncementBar> builder)
        {
            base.Configure(builder);
            builder.ToTable("AnnouncementBars");

            builder.Property(a => a.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.LinkUrl)
                .HasMaxLength(2048);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            // Index to efficiently fetch the single active announcement bar
            builder.HasIndex(a => a.IsActive)
                .HasDatabaseName("IX_AnnouncementBars_IsActive");
        }
    }
}
