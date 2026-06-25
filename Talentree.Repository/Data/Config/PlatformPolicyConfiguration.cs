using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    /// <summary>
    /// FR-AD-36: EF Core configuration for PlatformPolicy.
    /// Stores versioned legal documents. Content is nvarchar(max) to accommodate rich-text HTML.
    /// </summary>
    public class PlatformPolicyConfiguration : AuditableEntityConfiguration<PlatformPolicy>
    {
        public override void Configure(EntityTypeBuilder<PlatformPolicy> builder)
        {
            base.Configure(builder);
            builder.ToTable("PlatformPolicies");

            builder.Property(p => p.DocumentType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            // Unlimited length for rich HTML content
            builder.Property(p => p.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(p => p.VersionNumber)
                .IsRequired();

            builder.Property(p => p.IsPublished)
                .HasDefaultValue(false);

            builder.Property(p => p.RequireUserAcceptance)
                .HasDefaultValue(false);

            // Composite index for version history lookups
            builder.HasIndex(p => new { p.DocumentType, p.VersionNumber })
                .HasDatabaseName("IX_PlatformPolicies_DocumentType_VersionNumber");

            // Fast lookup for the currently published version per document type
            builder.HasIndex(p => new { p.DocumentType, p.IsPublished })
                .HasDatabaseName("IX_PlatformPolicies_DocumentType_IsPublished");
        }
    }
}
