using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    public class Complaint : AuditableEntity
    {
        public string ReportedUserId { get; set; } = null!; // User being complained about
        public string ReportedByUserId { get; set; } = null!; // User who filed complaint

        public ViolationType ViolationType { get; set; }
        public string Description { get; set; } = null!;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;

        // ✅ Keep as simple reference IDs (no navigation)
        public string? RelatedOrderId { get; set; } // Optional: link to order (as string for flexibility)
        public string? RelatedProductId { get; set; } // Optional: link to product
        public string? RelatedContext { get; set; } // Optional: additional context (e.g., "MaterialOrder", "ProductOrder", etc.)

        public string? ReviewedBy { get; set; } // Admin who reviewed
        public DateTime? ReviewedAt { get; set; }
        public string? Resolution { get; set; }
        public string? AdminNotes { get; set; }

        // Navigation - Only Users
        public AppUser ReportedUser { get; set; } = null!;
        public AppUser ReportedBy { get; set; } = null!;
    }
}