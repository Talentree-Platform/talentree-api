using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Repository.Data.Config.Base;

namespace Talentree.Repository.Data.Config
{
    public class ComplaintConfiguration : AuditableEntityConfiguration<Complaint>
    {
        public override void Configure(EntityTypeBuilder<Complaint> builder)
        {
            base.Configure(builder);

            builder.ToTable("Complaints");

            builder.Property(c => c.ReportedUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.ReportedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.ViolationType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(c => c.Description)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(c => c.Status)
                .HasConversion<int>()
                .HasDefaultValue(ComplaintStatus.Open);

            // ✅ Simple string references (no FK relationships)
            builder.Property(c => c.RelatedOrderId)
                .HasMaxLength(100);

            builder.Property(c => c.RelatedProductId)
                .HasMaxLength(100);

            builder.Property(c => c.RelatedContext)
                .HasMaxLength(100);

            builder.Property(c => c.ReviewedBy)
                .HasMaxLength(450);

            builder.Property(c => c.Resolution)
                .HasColumnType("nvarchar(max)");

            builder.Property(c => c.AdminNotes)
                .HasColumnType("nvarchar(max)");

            // ✅ Only User Relationships
            builder.HasOne(c => c.ReportedUser)
                .WithMany()
                .HasForeignKey(c => c.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.ReportedBy)
                .WithMany()
                .HasForeignKey(c => c.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(c => c.ReportedUserId)
                .HasDatabaseName("IX_Complaints_ReportedUserId");

            builder.HasIndex(c => c.Status)
                .HasDatabaseName("IX_Complaints_Status");

            builder.HasIndex(c => new { c.ReportedUserId, c.Status })
                .HasDatabaseName("IX_Complaints_ReportedUserId_Status");

            // ✅ Optional: Index for searching by order/product
            builder.HasIndex(c => c.RelatedOrderId)
                .HasDatabaseName("IX_Complaints_RelatedOrderId");

            builder.HasIndex(c => c.RelatedProductId)
                .HasDatabaseName("IX_Complaints_RelatedProductId");
        }
    }
}