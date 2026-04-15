using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class TicketMessageConfiguration : BaseEntityConfiguration<TicketMessage>
    {
        public override void Configure(EntityTypeBuilder<TicketMessage> builder)
        {
            base.Configure(builder);
            builder.ToTable("TicketMessages");

            builder.Property(m => m.SenderType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.Message)
                .IsRequired();

            builder.Property(m => m.AttachmentUrl)
                .HasMaxLength(500);

            builder.HasIndex(m => m.TicketId);
        }
    }
}