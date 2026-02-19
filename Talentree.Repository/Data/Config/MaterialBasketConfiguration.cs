using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class MaterialBasketConfiguration : IEntityTypeConfiguration<MaterialBasket>
    {
        public void Configure(EntityTypeBuilder<MaterialBasket> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450);

            // One basket per BO
            builder.HasIndex(b => b.BusinessOwnerId).IsUnique();

            builder.HasMany(b => b.Items)
                .WithOne(i => i.Basket)
                .HasForeignKey(i => i.BasketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
