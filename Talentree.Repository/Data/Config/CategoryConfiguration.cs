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
    public class CategoryConfiguration : SoftDeleteEntityConfiguration<Category>
    {
        public override void Configure(EntityTypeBuilder<Category> builder)
        {
            base.Configure(builder);
            builder.ToTable("Categories");

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.HasIndex(c => c.Name).IsUnique();

            builder.Property(c => c.BusinessType)
               .HasMaxLength(100);
        }
    }
}
