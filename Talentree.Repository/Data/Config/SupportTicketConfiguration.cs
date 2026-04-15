using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.Config
{
    public class SupportTicketConfiguration : BaseEntityConfiguration<SupportTicket>
    {
        public override void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            base.Configure(builder);
            builder.ToTable("SupportTickets");

            builder.Property(t => t.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(t => t.UserType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.Category)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Subject)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Open");

            builder.Property(t => t.Priority)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Medium");

            builder.Property(t => t.AutoCategory)
                .HasMaxLength(50);

            builder.Property(t => t.AssignedToAdminId)
                .HasMaxLength(450);

            // Relationships
            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(t => t.Messages)
                .WithOne(m => m.Ticket)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.Status);
        }
    }
}