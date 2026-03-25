
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config.Base
{
    public abstract class SoftDeleteEntityConfiguration<TEntity> : AuditableEntityConfiguration<TEntity>
        where TEntity : AuditableEntity, ISoftDelete
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // Apply base auditable configuration
            base.Configure(builder);

            builder.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.DeletedAt);

            builder.Property(e => e.DeletedBy)
                .HasMaxLength(450);


            builder.HasIndex(e => e.IsDeleted)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_IsDeleted");

            // Composite index for filtering deleted items
            builder.HasIndex(e => new { e.IsDeleted, e.DeletedAt })
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_IsDeleted_DeletedAt");

        }
    }
}