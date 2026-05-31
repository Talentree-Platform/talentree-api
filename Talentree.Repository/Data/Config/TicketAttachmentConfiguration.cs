using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
    {
        public void Configure(EntityTypeBuilder<TicketAttachment> builder)
        {
            builder.ToTable("TicketAttachments");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.TicketId).IsRequired();
            builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
            builder.Property(a => a.FileUrl).IsRequired().HasMaxLength(1000);
            builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
            builder.Property(a => a.UploadedBy).IsRequired().HasMaxLength(450);


            // Ticket relationship - RESTRICT to avoid multiple cascade paths
            builder.HasOne(a => a.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

            // Message relationship - SetNull (optional FK)
            builder.HasOne(a => a.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(a => a.TicketId)
                .HasDatabaseName("IX_TicketAttachments_TicketId");

            builder.HasIndex(a => a.MessageId)
                .HasDatabaseName("IX_TicketAttachments_MessageId");
        }
    }
}