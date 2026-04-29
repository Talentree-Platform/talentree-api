using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class TicketMessageConfiguration : AuditableEntityConfiguration<TicketMessage>
    {
        public override void Configure(EntityTypeBuilder<TicketMessage> builder)
        {
            base.Configure(builder);

            builder.ToTable("TicketMessages");

            builder.Property(m => m.TicketId).IsRequired();

            builder.Property(m => m.SenderId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(m => m.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(m => m.IsAdminMessage)
                .HasDefaultValue(false);

            builder.Property(m => m.EmailSent)
                .HasDefaultValue(false);

            builder.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(m => m.Attachments)
            //    .WithOne(a => a.Message)
            //    .HasForeignKey(a => a.MessageId)
            //    .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(m => m.TicketId)
                .HasDatabaseName("IX_TicketMessages_TicketId");

            builder.HasIndex(m => new { m.TicketId, m.CreatedAt })
                .HasDatabaseName("IX_TicketMessages_TicketId_CreatedAt");
        }
    }
}