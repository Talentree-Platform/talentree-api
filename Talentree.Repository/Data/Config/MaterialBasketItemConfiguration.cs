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
    public class MaterialBasketItemConfiguration : IEntityTypeConfiguration<MaterialBasketItem>
    {
        public void Configure(EntityTypeBuilder<MaterialBasketItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity).IsRequired();

            builder.HasOne(i => i.RawMaterial)
                .WithMany()
                .HasForeignKey(i => i.RawMaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
