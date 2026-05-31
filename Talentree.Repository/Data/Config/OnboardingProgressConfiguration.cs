using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talentree.Core.Entities.Identity;

namespace Talentree.Repository.Data.Config
{
    public class OnboardingProgressConfiguration : BaseEntityConfiguration<OnboardingProgress>
    {
        public override void Configure(EntityTypeBuilder<OnboardingProgress> builder)
        {
            base.Configure(builder);
            builder.ToTable("OnboardingProgress");

            builder.Property(o => o.BusinessOwnerId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(o => o.TourCompleted).HasDefaultValue(false);
            builder.Property(o => o.ChecklistProductAdded).HasDefaultValue(false);
            builder.Property(o => o.ChecklistPaymentSet).HasDefaultValue(false);
            builder.Property(o => o.ChecklistProfileDone).HasDefaultValue(false);

            // One per business owner
            builder.HasIndex(o => o.BusinessOwnerId).IsUnique();

            // Relationship
            builder.HasOne(o => o.BusinessOwner)
                .WithMany()
                .HasForeignKey(o => o.BusinessOwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}