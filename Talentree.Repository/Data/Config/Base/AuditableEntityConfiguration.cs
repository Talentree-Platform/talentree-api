
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config.Base
{
    public abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : AuditableEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {

            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()"); 

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(450); 

            builder.Property(e => e.UpdatedAt);

            builder.Property(e => e.UpdatedBy)
                .HasMaxLength(450);


            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedAt");

            // Composite index for filtering by creator
            builder.HasIndex(e => new { e.CreatedBy, e.CreatedAt })
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedBy_CreatedAt");
        }
    }
}