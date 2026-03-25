using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class MaterialBasketConfiguration : AuditableEntityConfiguration<MaterialBasket>
    {
        public override void Configure(EntityTypeBuilder<MaterialBasket> builder)
        {
            base.Configure(builder);
            builder.HasKey(b => b.Id);

            builder.Property(b => b.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450); // Identity PK length

            // One basket per BO
            builder.HasIndex(b => b.BusinessOwnerId)
                .IsUnique();

            builder.HasMany(b => b.Items)
                .WithOne(i => i.Basket)
                .HasForeignKey(i => i.BasketId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting basket removes all its items
        }
    }
}
