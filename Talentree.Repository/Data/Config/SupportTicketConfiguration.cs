using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class SupportTicketConfiguration : SoftDeleteEntityConfiguration<SupportTicket>
    {
        public override void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            base.Configure(builder);

            builder.ToTable("SupportTickets");

            builder.Property(t => t.TicketNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Subject)
                .IsRequired()
                .HasMaxLength(255);

           
            builder.Property(t => t.Description)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(t => t.Category)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.Status)
                .HasConversion<int>()
                .HasDefaultValue(TicketStatus.Open);

            builder.Property(t => t.Priority)
                .HasConversion<int>()
                .HasDefaultValue(TicketPriority.Normal);

            builder.Property(t => t.BusinessOwnerUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(t => t.AssignedToAdminId)
                .HasMaxLength(450);

            builder.Property(t => t.ResolvedBy)
                .HasMaxLength(450);

            builder.Property(t => t.ClosedBy)
                .HasMaxLength(450);

            builder.HasOne(t => t.BusinessOwner)
                .WithMany()
                .HasForeignKey(t => t.BusinessOwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Messages)
                .WithOne(m => m.Ticket)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(t => t.Attachments)
            //    .WithOne(a => a.Ticket)
            //    .HasForeignKey(a => a.TicketId)
            //    .OnDelete(DeleteBehavior.Restrict);
            //    //.OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.TicketNumber)
                .IsUnique()
                .HasDatabaseName("IX_SupportTickets_TicketNumber");

            builder.HasIndex(t => t.BusinessOwnerUserId)
                .HasDatabaseName("IX_SupportTickets_BusinessOwnerUserId");

            builder.HasIndex(t => t.Status)
                .HasDatabaseName("IX_SupportTickets_Status");

            builder.HasIndex(t => new { t.Status, t.CreatedAt })
                .HasDatabaseName("IX_SupportTickets_Status_CreatedAt");

            builder.HasIndex(t => t.AssignedToAdminId)
                .HasDatabaseName("IX_SupportTickets_AssignedToAdminId");
        }
    }
}