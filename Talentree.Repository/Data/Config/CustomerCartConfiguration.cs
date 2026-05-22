using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class CustomerCartConfiguration : AuditableEntityConfiguration<CustomerCart>
    {
        public override void Configure(EntityTypeBuilder<CustomerCart> builder)
        {
            base.Configure(builder);
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CustomerId)
                .IsRequired()
                .HasMaxLength(450);

            // One cart per customer
            builder.HasIndex(c => c.CustomerId)
                .IsUnique();

            builder.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
