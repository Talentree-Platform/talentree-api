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
    public class ProductImageConfiguration : BaseEntityConfiguration<ProductImage>
    {
        public override void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            base.Configure(builder);
            builder.ToTable("ProductImages");

            builder.Property(i => i.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.IsMain)
                .HasDefaultValue(false);

            builder.HasIndex(i => i.ProductId);
        }
    }
}
