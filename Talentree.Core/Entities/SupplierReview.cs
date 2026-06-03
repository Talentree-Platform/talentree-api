using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    public class SupplierReview : AuditableEntity, ISoftDelete
    {
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; } = null!;

        public string BusinessOwnerUserId { get; set; } = null!;
        public AppUser BusinessOwner { get; set; } = null!;

        public int MaterialOrderId { get; set; }
        public MaterialOrder MaterialOrder { get; set; } = null!;

        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
