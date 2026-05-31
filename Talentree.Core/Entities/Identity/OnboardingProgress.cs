namespace Talentree.Core.Entities.Identity
{
    public class OnboardingProgress : BaseEntity
    {
        public string BusinessOwnerId { get; set; } = null!;
        public AppUser BusinessOwner { get; set; } = null!;

        public bool TourCompleted { get; set; } = false;
        public bool ChecklistProductAdded { get; set; } = false;
        public bool ChecklistPaymentSet { get; set; } = false;
        public bool ChecklistProfileDone { get; set; } = false;
    }
}